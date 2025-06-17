// <copyright file="Spreadsheet.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

// Written by Joe Zachary for CS 3500, September 2013
// Update by Profs Kopta and de St. Germain, Fall 2021, Fall 2024
//     - Updated return types
//     - Updated documentation

// Complete implementation written by Logan Wood for CS 3500
// Version for PS5 written September 27, 2024
// Version for PS6 written October 18, 2024


namespace CS3500.Spreadsheet;

using CS3500.Formula;
using CS3500.DependencyGraph;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Xml.Linq;
using Microsoft.VisualBasic;

/// <summary>
/// <para>
///   Thrown to indicate that a read or write attempt has failed with
///   an expected error message informing the user of what went wrong.
/// </para>
/// </summary>
public class SpreadsheetReadWriteException : Exception
{
    /// <summary>
    ///   <para>
    ///     Creates the exception with a message defining what went wrong.
    ///   </para>
    /// </summary>
    /// <param name="msg"> An informative message to the user. </param>
    public SpreadsheetReadWriteException(string msg)
    : base(msg)
    {
    }
}

/// <summary>
///   <para>
///     Thrown to indicate that a change to a cell will cause a circular dependency.
///   </para>
/// </summary>
public class CircularException : Exception
{
}

/// <summary>
///   <para>
///     Thrown to indicate that a name parameter was invalid.
///   </para>
/// </summary>
public class InvalidNameException : Exception
{
}

/// <summary>
///   <para>
///     An Spreadsheet object represents the state of a simple spreadsheet.  A
///     spreadsheet represents an infinite number of named cells.
///   </para>
/// <para>
///     Valid Cell Names: A string is a valid cell name if and only if it is one or
///     more letters followed by one or more numbers, e.g., A5, BC27.
/// </para>
/// <para>
///    Cell names are case insensitive, so "x1" and "X1" are the same cell name.
///    Your code should normalize (uppercased) any stored name but accept either.
/// </para>
/// <para>
///     A spreadsheet represents a cell corresponding to every possible cell name.  (This
///     means that a spreadsheet contains an infinite number of cells.)  In addition to
///     a name, each cell has a contents and a value.  The distinction is important.
/// </para>
/// <para>
///     The <b>contents</b> of a cell can be (1) a string, (2) a double, or (3) a Formula.
///     If the contents of a cell is set to the empty string, the cell is considered empty.
/// </para>
/// <para>
///     By analogy, the contents of a cell in Excel is what is displayed on
///     the editing line when the cell is selected.
/// </para>
/// <para>
///     In a new spreadsheet, the contents of every cell is the empty string. Note:
///     this is by definition (it is IMPLIED, not stored).
/// </para>
/// <para>
///     The <b>value</b> of a cell can be (1) a string, (2) a double, or (3) a FormulaError.
///     (By analogy, the value of an Excel cell is what is displayed in that cell's position
///     in the grid.) We are not concerned with cell values yet, only with their contents,
///     but for context:
/// </para>
/// <list type="number">
///   <item>If a cell's contents is a string, its value is that string.</item>
///   <item>If a cell's contents is a double, its value is that double.</item>
///   <item>
///     <para>
///       If a cell's contents is a Formula, its value is either a double or a FormulaError,
///       as reported by the Evaluate method of the Formula class.  For this assignment,
///       you are not dealing with values yet.
///     </para>
///   </item>
/// </list>
/// <para>
///     Spreadsheets are never allowed to contain a combination of Formulas that establish
///     a circular dependency.  A circular dependency exists when a cell depends on itself,
///     either directly or indirectly.
///     For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
///     A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
///     dependency.
/// </para>
/// </summary>
public class Spreadsheet
{

    /// <summary> Mapping of all nonempty cells </summary>
    [JsonInclude]
    [JsonPropertyName("Cells")]
    private Dictionary<string, Cell> cells;
    /// <summary> Cell dependencies </summary>
    private DependencyGraph dependencies;

    // Constructor initializes the dictionary and dependency graph
    public Spreadsheet()
    {
        cells = new Dictionary<string, Cell>();
        dependencies = new DependencyGraph();
        Changed = false;
    }

    /// <summary>
    /// Validates the cell name according to the rules specified.
    /// </summary>
    private bool IsValidCellName(string name)
    {
        if(string.IsNullOrWhiteSpace(name)) return false;

        // Regular expression: One or more letters followed by one or more digits.
        return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z]+\d+$");
    }

    /// <summary>
    /// Normalizes the cell name by converting it to uppercase.
    /// </summary>
    private string Normalize(string name)
    {
        return name.ToUpper();
    }


    /// <summary>
    ///   Provides a copy of the normalized names of all of the cells in the spreadsheet
    ///   that contain information (i.e., non-empty cells).
    /// </summary>
    /// <returns>
    ///   A set of the names of all the non-empty cells in the spreadsheet.
    /// </returns>
    public ISet<string> GetNamesOfAllNonemptyCells()
    {
        return cells.Keys.ToHashSet();
    }

    /// <summary>
    ///   Returns the contents (as opposed to the value) of the named cell.
    /// </summary>
    ///
    /// <exception cref="InvalidNameException">
    ///   Thrown if the name is invalid.
    /// </exception>
    ///
    /// <param name="name">The name of the spreadsheet cell to query. </param>
    /// <returns>
    ///   The contents as either a string, a double, or a Formula.
    ///   See the class header summary.
    /// </returns>
    public object GetCellContents(string name)
    {
        name = Normalize(name); //Normalize name

        //Invalid cell name case
        if(!IsValidCellName(name))
            throw new InvalidNameException();

        //Empty cell case
        if (!cells.ContainsKey(name))
            return string.Empty;
        else
            return cells[name].contents;
    }


    /// <summary>
    ///  Set the contents of the named cell to the given number.
    /// </summary>
    ///
    /// <exception cref="InvalidNameException">
    ///   If the name is invalid, throw an InvalidNameException.
    /// </exception>
    ///
    /// <param name="name"> The name of the cell. </param>
    /// <param name="number"> The new contents of the cell. </param>
    /// <returns>
    ///   <para>
    ///     This method returns an ordered list consisting of the passed in name
    ///     followed by the names of all other cells whose value depends, directly
    ///     or indirectly, on the named cell.
    ///   </para>
    ///   <para>
    ///     The order must correspond to a valid dependency ordering for recomputing
    ///     all of the cells, i.e., if you re-evaluate each cells in the order of the list,
    ///     the overall spreadsheet will be correctly updated.
    ///   </para>
    ///   <para>
    ///     For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    ///     list [A1, B1, C1] is returned, i.e., A1 was C, so then A1 must be
    ///     evaluated, followed by B1, followed by C1.
    ///   </para>
    /// </returns>
    private IList<string> SetCellContents(string name, double number)
    {
        //Cell is not already in dictionary
        if (!cells.ContainsKey(name))
            cells.Add(name, new Cell(number));
        //Cell is in dictionary; update cell contents
        else
            cells[name].contents = number;

        return GetCellsToRecalculate(name).ToList();
    }

    /// <summary>
    ///   The contents of the named cell becomes the given text.
    /// </summary>
    ///
    /// <exception cref="InvalidNameException">
    ///   If the name is invalid, throw an InvalidNameException.
    /// </exception>
    /// <param name="name"> The name of the cell. </param>
    /// <param name="text"> The new contents of the cell. </param>
    /// <returns>
    ///   The same list as defined in <see cref="SetCellContents(string, double)"/>.
    /// </returns>
    private IList<string> SetCellContents(string name, string text)
    {
        //Cell is not already in dictionary
        if (!cells.ContainsKey(name))
            cells.Add(name, new Cell(text));
        //Cell is in dictionary; update cell contents
        else
            cells[name].contents = text;

        if(IsBlankCell(name))
        {
            //Remove the cell if it is an empty white cell
            cells.Remove(name);
        }

        return GetCellsToRecalculate(name).ToList();
    }

    /// <summary>
    ///   Set the contents of the named cell to the given formula.
    /// </summary>
    /// <exception cref="InvalidNameException">
    ///   If the name is invalid, throw an InvalidNameException.
    /// </exception>
    /// <exception cref="CircularException">
    ///   <para>
    ///     If changing the contents of the named cell to be the formula would
    ///     cause a circular dependency, throw a CircularException, and no
    ///     change is made to the spreadsheet.
    ///   </para>
    /// </exception>
    /// <param name="name"> The name of the cell. </param>
    /// <param name="formula"> The new contents of the cell. </param>
    /// <returns>
    ///   The same list as defined in <see cref="SetCellContents(string, double)"/>.
    /// </returns>
    private IList<string> SetCellContents(string name, Formula formula)
    {
        //Backup the current dependencies for undoing if needed
        var oldDependees = new HashSet<string>(dependencies.GetDependees(name));

        //Replace dependees with new dependency pairs temporarily
        dependencies.ReplaceDependees(name, formula.GetVariables());

        //Check for circular dependencies
        try
        {
            //Check if setting the new formula causes a circular dependency
            var cellsToRecalculate = GetCellsToRecalculate(name);
            
            //Update the cell's content

            //Cell is not already in dictionary
            if (!cells.ContainsKey(name))
                cells.Add(name, new Cell(formula));
            //Cell is in dictionary; update cell contents
            else
                cells[name].contents = formula;

            return cellsToRecalculate.ToList();
        }
        catch (CircularException)
        {
            //If circular exception is caught, undo the graph changes
            dependencies.ReplaceDependees(name, oldDependees);
            throw; //Re-throw the exception 
        }
    }

    /// <summary>
    ///   Returns an enumeration, without duplicates, of the names of all cells whose
    ///   values depend directly on the value of the named cell.
    /// </summary>
    /// <param name="name"> This <b>MUST</b> be a valid name.  </param>
    /// <returns>
    ///   <para>
    ///     Returns an enumeration, without duplicates, of the names of all cells
    ///     that contain formulas containing name.
    ///   </para>
    ///   <para>For example, suppose that: </para>
    ///   <list type="bullet">
    ///      <item>A1 contains 3</item>
    ///      <item>B1 contains the formula A1 * A1</item>
    ///      <item>C1 contains the formula B1 + A1</item>
    ///      <item>D1 contains the formula B1 - C1</item>
    ///   </list>
    ///   <para> The direct dependents of A1 are B1 and C1. </para>
    /// </returns>
    private IEnumerable<string> GetDirectDependents(string name)
    {
        return dependencies.GetDependents(Normalize(name));
    }

    /// <summary>
    ///   <para>
    ///     This method is implemented for you, but makes use of your GetDirectDependents.
    ///   </para>
    ///   <para>
    ///     Returns an enumeration of the names of all cells whose values must
    ///     be recalculated, assuming that the contents of the cell referred
    ///     to by name has changed.  The cell names are enumerated in an order
    ///     in which the calculations should be done.
    ///   </para>
    ///   <exception cref="CircularException">
    ///     If the cell referred to by name is involved in a circular dependency,
    ///     throws a CircularException.
    ///   </exception>
    ///   <para>
    ///     For example, suppose that:
    ///   </para>
    ///   <list type="number">
    ///     <item>
    ///       A1 contains 5
    ///     </item>
    ///     <item>
    ///       B1 contains the formula A1 + 2.
    ///     </item>
    ///     <item>
    ///       C1 contains the formula A1 + B1.
    ///     </item>
    ///     <item>
    ///       D1 contains the formula A1 * 7.
    ///     </item>
    ///     <item>
    ///       E1 contains 15
    ///     </item>
    ///   </list>
    ///   <para>
    ///     If A1 has changed, then A1, B1, C1, and D1 must be recalculated,
    ///     and they must be recalculated in an order which has A1 first, and B1 before C1
    ///     (there are multiple such valid orders).
    ///     The method will produce one of those enumerations.
    ///   </para>
    ///   <para>
    ///      PLEASE NOTE THAT THIS METHOD DEPENDS ON THE METHOD GetDirectDependents.
    ///      IT WON'T WORK UNTIL GetDirectDependents IS IMPLEMENTED CORRECTLY.
    ///   </para>
    /// </summary>
    /// <param name="name"> The name of the cell.  Requires that name be a valid cell name.</param>
    /// <returns>
    ///    Returns an enumeration of the names of all cells whose values must
    ///    be recalculated.
    /// </returns>
    private IEnumerable<string> GetCellsToRecalculate(string name)
    {
        LinkedList<string> changed = new();
        HashSet<string> visited = [];
        Visit(name, name, visited, changed);
        return changed;
    }

    /// <summary>
    ///   A helper for the GetCellsToRecalculate method.
    ///   <para>
    ///   Recursively "visits" each dependent cell (direct and indirect) of the cell given by the 
    ///   parameter "name", adding the name of each dependent cell to a list of "changed" and "visited"
    ///   cells. This produces the list of all direct and indirect dependents of the starting cell.
    ///   </para>
    ///   
    /// <exception cref="CircularException">
    ///     If the cell referred to by name is involved in a circular dependency,
    ///     throws a CircularException.
    /// </exception>
    /// 
    /// </summary>
    private void Visit(string start, string name, ISet<string> visited, LinkedList<string> changed)
    {
        visited.Add(name); //Add this cell to the list of visited cells
        //Loop through all of the direct dependents of this cell
        foreach (string n in GetDirectDependents(name))
        {
            //Case for if this cell creates a loop with the starting cell
            if (n.Equals(start))
            {
                throw new CircularException();
            }
            //If this cell has not already been visited...
            else if (!visited.Contains(n))
            {
                //Visit this cell and its direct dependents
                Visit(start, n, visited, changed);
            }
        }

        //Add this cell to the list of cells which will need to be recalculated
        changed.AddFirst(name);
    }

    /// <summary>
    /// This class represents an individual cell within the spreadsheet.
    /// </summary>
    private class Cell
    {
        public Cell(object contents)
        {
            this.contents = contents;
            this.StringForm = GetCellStringForm(contents);
            this.Color = "white";
        }

        [JsonConstructor]
        public Cell(string StringForm, string Color)
        {
            this.StringForm = StringForm;
            this.Color = Color;
            contents = "";
            value = "";
        }

        [JsonIgnore]
        public object contents;
        [JsonIgnore]
        public object value = "";

        /// <summary> The contents of this cell in string form </summary>
        public string StringForm {
            get;
            set;
        }

        /// <summary> The color of the cell in string form </summary>
        public string Color
        {
            get;
            set;
        }
    }


    //CODE ADDED FOR PS6

    /// <summary>
    ///   <para>
    ///     Return the value of the named cell, as defined by
    ///     <see cref="GetCellValue(string)"/>.
    ///   </para>
    /// </summary>
    /// <param name="name"> The cell in question. </param>
    /// <returns>
    ///   <see cref="GetCellValue(string)"/>
    /// </returns>
    /// <exception cref="InvalidNameException">
    ///   If the provided name is invalid, throws an InvalidNameException.
    /// </exception>
    public object this[string name]
    {
        get {
            //Invalid cell name case
            if (!IsValidCellName(name))
                throw new InvalidNameException();
            //GetCellValue
            return cells.ContainsKey(name) ? cells[name].value : string.Empty;
        }
    }


    /// <summary>
    /// True if this spreadsheet has been changed since it was 
    /// created or saved (whichever happened most recently),
    /// False otherwise.
    /// </summary>
    [JsonIgnore]
    public bool Changed { get; private set; }


    /// <summary>
    /// Constructs a spreadsheet using the saved data in the file refered to by
    /// the given filename. 
    /// <see cref="Save(string)"/>
    /// </summary>
    /// <exception cref="SpreadsheetReadWriteException">
    ///   Thrown if the file can not be loaded into a spreadsheet for any reason
    /// </exception>
    /// <param name="filename">The path to the file containing the spreadsheet to load</param>
    public Spreadsheet(string filename)
    {
        //Initialize spreadsheet variables
        cells = new Dictionary<string, Cell>();
        dependencies = new DependencyGraph();

        //Read file
        try
        {
            // Load the file and deserialize the spreadsheet data
            string jsonData = File.ReadAllText(filename);
            //var data = JsonSerializer.Deserialize<Dictionary<string, Cell>>(jsonData)!;
            var data = JsonSerializer.Deserialize<Spreadsheet>(jsonData)!;

            this.cells = data.cells;
            this.dependencies = data.dependencies;

            //Using the loaded stringForm data, set the contents of the corresponding cells
            foreach (string cell in data.cells.Keys)
                SetContentsOfCell(cell, data.cells[cell].StringForm);
        }
        // Throw exception if the file can't be loaded properly
        catch (Exception ex)
        {
            throw new SpreadsheetReadWriteException("Error reading spreadsheet file: " + ex.Message);
        }

        Changed = false;
    }


    /// <summary>
    ///   <para>
    ///     Writes the contents of this spreadsheet to the named file using a JSON format.
    ///     If the file already exists, overwrite it.
    ///   </para>
    ///   <para>
    ///     The output JSON should look like the following.
    ///   </para>
    ///   <para>
    ///     For example, consider a spreadsheet that contains a cell "A1" 
    ///     with contents being the double 5.0, and a cell "B3" with contents 
    ///     being the Formula("A1+2"), and a cell "C4" with the contents "hello".
    ///   </para>
    ///   <para>
    ///      This method would produce the following JSON string:
    ///   </para>
    ///   <code>
    ///   {
    ///     "Cells": {
    ///       "A1": {
    ///         "StringForm": "5"
    ///       },
    ///       "B3": {
    ///         "StringForm": "=A1+2"
    ///       },
    ///       "C4": {
    ///         "StringForm": "hello"
    ///       }
    ///     }
    ///   }
    ///   </code>
    ///   <para>
    ///     You can achieve this by making sure your data structure is a dictionary 
    ///     and that the contained objects (Cells) have property named "StringForm"
    ///     (if this name does not match your existing code, use the JsonPropertyName 
    ///     attribute).
    ///   </para>
    ///   <para>
    ///     There can be 0 cells in the dictionary, resulting in { "Cells" : {} } 
    ///   </para>
    ///   <para>
    ///     Further, when writing the value of each cell...
    ///   </para>
    ///   <list type="bullet">
    ///     <item>
    ///       If the contents is a string, the value of StringForm is that string
    ///     </item>
    ///     <item>
    ///       If the contents is a double d, the value of StringForm is d.ToString()
    ///     </item>
    ///     <item>
    ///       If the contents is a Formula f, the value of StringForm is "=" + f.ToString()
    ///     </item>
    ///   </list>
    /// </summary>
    /// <param name="filename"> The name (with path) of the file to save to.</param>
    /// <exception cref="SpreadsheetReadWriteException">
    ///   If there are any problems opening, writing, or closing the file, 
    ///   the method should throw a SpreadsheetReadWriteException with an
    ///   explanatory message.
    /// </exception>
    public void Save(string filename)
    {
        if (Changed) { 
            try {
                //Write file
                File.WriteAllText(filename, GetJsonString());
            }
            //Catch any exeptions which occur when opening, writing, or closing the file
            catch (Exception e) {
                throw new SpreadsheetReadWriteException("Error saving spreadsheet to file: " + e.Message);
            }
            Changed = false; // Reset changed to false
        }   
    }

    /// <summary>
    ///   <para>
    ///     Return the value of the named cell.
    ///   </para>
    /// </summary>
    /// <param name="name"> The cell in question. </param>
    /// <returns>
    ///   Returns the value (as opposed to the contents) of the named cell.  The return
    ///   value should be either a string, a double, or a CS3500.Formula.FormulaError.
    /// </returns>
    /// <exception cref="InvalidNameException">
    ///   If the provided name is invalid, throws an InvalidNameException.
    /// </exception>
    public object GetCellValue(string name)
    {
        //Normalize cell name
        name = Normalize(name);
        //Invalid cell name case
        if (!IsValidCellName(name))
            throw new InvalidNameException();

        var content = GetCellContents(name);
        //if(cells.TryGetValue(name, out var cell))

        // Case for cells containing a Formula
        if (content.GetType().Equals(typeof(Formula)))
        {
            return ((Formula)content).Evaluate(CellLookup);
        }
        return content;
    }

    /// <summary>
    /// Gets the StringForm of the given cell. i.e. the contents of the cell in string form.
    /// </summary>
    /// <param name="name"> Cell contents</param>
    /// <returns> Cell content in the form of a string </returns>
    public static string GetCellStringForm(object contents)
    {
        //var contents = GetCellContents(name);
        if (contents.GetType().Equals(typeof(Formula)))
            return "=" + ((Formula)contents).ToString();
        else if (contents.GetType().Equals(typeof(double)))
            return ((double)contents).ToString();
        else
            return (string)contents;
    }

    /// <summary>
    ///   <para>
    ///     Set the contents of the named cell to be the provided string
    ///     which will either represent (1) a string, (2) a number, or 
    ///     (3) a formula (based on the prepended '=' character).
    ///   </para>
    ///   <para>
    ///     Rules of parsing the input string:
    ///   </para>
    ///   <list type="bullet">
    ///     <item>
    ///       <para>
    ///         If 'content' parses as a double, the contents of the named
    ///         cell becomes that double.
    ///       </para>
    ///     </item>
    ///     <item>
    ///         If the string does not begin with an '=', the contents of the 
    ///         named cell becomes 'content'.
    ///     </item>
    ///     <item>
    ///       <para>
    ///         If 'content' begins with the character '=', an attempt is made
    ///         to parse the remainder of content into a Formula f using the Formula
    ///         constructor.  There are then three possibilities:
    ///       </para>
    ///       <list type="number">
    ///         <item>
    ///           If the remainder of content cannot be parsed into a Formula, a 
    ///           CS3500.Formula.FormulaFormatException is thrown.
    ///         </item>
    ///         <item>
    ///           Otherwise, if changing the contents of the named cell to be f
    ///           would cause a circular dependency, a CircularException is thrown,
    ///           and no change is made to the spreadsheet.
    ///         </item>
    ///         <item>
    ///           Otherwise, the contents of the named cell becomes f.
    ///         </item>
    ///       </list>
    ///     </item>
    ///   </list>
    /// </summary>
    /// <returns>
    ///   <para>
    ///     The method returns a list consisting of the name plus the names 
    ///     of all other cells whose value depends, directly or indirectly, 
    ///     on the named cell. The order of the list should be any order 
    ///     such that if cells are re-evaluated in that order, their dependencies 
    ///     are satisfied by the time they are evaluated.
    ///   </para>
    ///   <example>
    ///     For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    ///     list {A1, B1, C1} is returned.
    ///   </example>
    /// </returns>
    /// <exception cref="InvalidNameException">
    ///     If name is invalid, throws an InvalidNameException.
    /// </exception>
    /// <exception cref="CircularException">
    ///     If a formula would result in a circular dependency, throws CircularException.
    /// </exception>
    public IList<string> SetContentsOfCell(string name, string content)
    {
        name = Normalize(name); //Normalize name
        //Invalid cell name case
        if (!IsValidCellName(name))
            throw new InvalidNameException();

        //Call SetCellContent depending on the type of content in the cell

        IList<string> changedCells;

        //Formula case
        if (content.StartsWith('='))
            changedCells = SetCellContents(name, new Formula(content.TrimStart('=')));
        //Double case
        else if (double.TryParse(content, out double result))
            changedCells = SetCellContents(name, result);
        //String case
        else
            changedCells = SetCellContents(name, content);

        //Update value of changed cells
        foreach (string cellName in changedCells)
        {
            if(cells.TryGetValue(cellName, out Cell? cell))
                cell.value = GetCellValue(cellName);
        }

        Changed = true;

        return changedCells;
    }

    /// <summary>
    ///     Sets the backing color of the given cell to the given color
    /// </summary>
    /// <param name="name"> Name of cell </param>
    /// <param name="color"> Backing color </param>
    /// <exception cref="InvalidNameException"></exception>
    public void SetCellColor(string name, string color)
    {
        //Normalize cell name
        name = Normalize(name);
        //Invalid cell name case
        if (!IsValidCellName(name))
            throw new InvalidNameException();

        //Cell is not already in dictionary
        if (!cells.ContainsKey(name))
            cells.Add(name, new Cell(""));
        //Set cell color
        cells[name].Color = color;

        //Case to remove the cell from cells if it is an empty white cell
        if(IsBlankCell(name))
            //Remove the cell if it is an empty white cell
            cells.Remove(name);

    }

    /// <summary>
    ///     Gets the color of the given cell
    /// </summary>
    /// <param name="name"> Name of cell </param>
    /// <returns> Color of the given cell </returns>
    /// <exception cref="InvalidNameException"></exception>
    public string GetCellColor(string name)
    {
        //Normalize cell name
        name = Normalize(name);
        //Invalid cell name case
        if (!IsValidCellName(name))
            throw new InvalidNameException();

        if(GetNamesOfAllNonemptyCells().Contains(name))
            return cells[name].Color;
        else
            return "white"; //Default color
    }

    /// <summary>
    ///     Determines if the given cell is blank  (i.e. no content and unchanged color)
    /// </summary>
    /// <param name="name"> Cell name </param>
    /// <returns> Whether or not the given cell is blank </returns>
    private bool IsBlankCell(string name)
    {
        if (GetCellContents(name).Equals(string.Empty) && GetCellColor(name).Equals("white"))
            return true;
        return false;
    }

    /// <summary>
    /// Returns the json string representing this spreadsheet.
    /// </summary>
    /// <returns></returns>
    public string GetJsonString()
    {
        //Json Options
        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };

        //Serialize and write to file
        return JsonSerializer.Serialize(this, jsonOptions);
    }

    /// <summary>
    /// Replaces the existing spreadsheet with a new one represented by a json string.
    /// </summary>
    /// <param name="jsonString"> JSON string form of the spreadsheet </param>
    /// <exception cref="SpreadsheetReadWriteException"></exception>
    public IList<string> SetSpreadsheetFromJson(string jsonString)
    {
        //Get the previous cells before loading new ones
        var oldCells = GetNamesOfAllNonemptyCells();

        //Initialize spreadsheet variables
        cells = new Dictionary<string, Cell>();
        dependencies = new DependencyGraph();

        //Read file
        try
        {
            var data = JsonSerializer.Deserialize<Spreadsheet>(jsonString)!;

            this.cells = data.cells;
            this.dependencies = data.dependencies;

            //Using the loaded stringForm data, set the contents of the corresponding cells
            foreach (string cell in data.cells.Keys)
                SetContentsOfCell(cell, data.cells[cell].StringForm);
        }
        // Throw exception if the file can't be loaded properly
        catch (Exception ex)
        {
            throw new SpreadsheetReadWriteException("Error reading spreadsheet file: " + ex.Message);
        }

        Changed = false;

        //Combine the old cells with the new cells to get a set of all the cells which now need updating
        oldCells.UnionWith(GetNamesOfAllNonemptyCells()); 
        return oldCells.ToList<string>();
    }


    /// <summary>
    /// Used for the Lookup parameter of Formula.Evaluate. 
    /// </summary>
    /// <param name="name"> Name of the cell to look up (e.g. "A1")</param>
    /// <returns>A double representing the value of this cell</returns>
    /// <exception cref="ArgumentException"> Throws if the value is of the cell is not a double</exception>
    private double CellLookup(string name)
    {
        if (GetCellValue(name).GetType() == typeof(double))
            return (double)GetCellValue(name);
        else if ((string) GetCellContents(name) == string.Empty) return 0.0;
        throw new ArgumentException();
    }
}
