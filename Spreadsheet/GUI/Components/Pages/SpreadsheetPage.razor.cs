// <copyright file="SpreadsheetPage.razor.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>
// <authors>
//   Logan Wood, Dylan Kelly
// </authors>
// <version>
//   October 26, 2024
// </version>

namespace GUI.Client.Pages;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.Xml.Serialization;
using CS3500.Spreadsheet;
using CS3500.Formula;
using System;
using System.Drawing;
using System.Xml.Linq;

/// <summary>
///    <para> Creates a spreadsheet GUI with 26 Columns and 50 Rows. </para> 
///    <para> Includes the ability to create formulas, save and load contents, and change the color of a cell </para>
/// </summary>
public partial class SpreadsheetPage
{
    Spreadsheet sheet = new();

    //cc prefix stands for "current cell"
    //pc prefix stands for "previous cell"

    private int cc_CellRow = 0;     //Row of currently selected cell
    private int cc_CellCol = 0;     //Col of currently selected cell
    private int pc_CellRow = -1;     //Row of previously selected cell
    private int pc_CellCol = -1;     //Col of previously selected cell

    private string cc_name = "A1";      //Default is A1
    private string cc_ContentText = ""; 
    private string cc_ValueText = "";

    private string cc_Color = "white";

    private bool hasError = false;
    private Exception errorText = null;

    /// <summary>
    /// Based on your computer, you could shrink/grow this value based on performance.
    /// </summary>
    private const int ROWS = 50;

    /// <summary>
    /// Number of columns, which will be labeled A-Z.
    /// </summary>
    private const int COLS = 26;

    /// <summary>
    /// Provides an easy way to convert from an index to a letter (0 -> A)
    /// </summary>
    private char[] Alphabet { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();


    /// <summary>
    ///   Gets or sets the name of the file to be saved
    /// </summary>
    private string FileSaveName { get; set; } = "Spreadsheet.sprd";


    /// <summary>
    ///   <para> Gets or sets the data for all of the cells in the spreadsheet GUI. </para>
    ///   <remarks>Backing Store for HTML</remarks>
    /// </summary>
    private string[,] CellsBackingStore { get; set; } = new string[ROWS, COLS];


    /// <summary>
    ///    <para> Gets or sets the color for all of the cells in the spreadsheet GUI. </para>
    /// </summary>
    private string[,] CellsColorStore { get; set; } = new string[ROWS, COLS];

    /// <summary>
    ///     <para> Gets or sets the border style for all of the cells in the spreadsheet GUI. </para>
    /// </summary>
    private string[,] CellBorderStyles { get; set; } = new string[ROWS, COLS];


    /// <summary>
    /// Handler for when a cell is clicked
    /// </summary>
    /// <param name="row">The row component of the cell's coordinates</param>
    /// <param name="col">The column component of the cell's coordinates</param>
    private void CellClicked( int row, int col )
    {
        if(pc_CellCol >= 0 && pc_CellCol >= 0){
            //Update anything related to the previously selected cell
            CellBorderStyles[pc_CellRow, pc_CellCol] = "";
        }

        
        //Update currently selected row and col
        cc_CellRow = row;
        cc_CellCol = col;

        //Update currently selected cell's name and content
        cc_name = Alphabet[col] + "" + (row+1); 
        cc_ValueText = CellsBackingStore[row, col];
        var c = string.Empty;
        var content = sheet.GetCellContents(cc_name);
        cc_ContentText = content.GetType().Equals(typeof(Formula)) ? "="+content.ToString(): content.ToString()!;
        cc_Color = CellsColorStore[row, col];
        CellBorderStyles[row, col] = "3px solid black";

        //Update previous cell information
        pc_CellCol = col;
        pc_CellRow = row;
    }

    /// <summary>
    ///     This private helper handles changing the backing color of a cell
    /// </summary>
    /// <param name="e"> Change Event </param>
    private void HandleColorChange(ChangeEventArgs e)
    {
        cc_Color = e.Value.ToString();
        sheet.SetCellColor(cc_name, cc_Color);
        //CellsColorStore[cc_CellRow, cc_CellCol] = cc_Color;
        CellsColorStore[cc_CellRow, cc_CellCol] = sheet.GetCellColor(cc_name);
        StateHasChanged();
    }

    /// <summary>
    ///     <para> When enter is pressed, sets the contents of the selected cell. </para>
    ///     <para> Does not update contents of cell when an error is thrown (i.e. FormulaFormatException()) </para>
    /// </summary>
    /// <param name="e"> Keyboard Event (i.e. key pressed) </param>
    private void EnterContents(KeyboardEventArgs e)
    {
        Debug.WriteLine(cc_ContentText);

        if (e.Key == "Enter")
        {
            try
            {
                Debug.WriteLine(cc_ContentText);
                var changedCells = sheet.SetContentsOfCell(cc_name, cc_ContentText);
                cc_ValueText = sheet.GetCellValue(cc_name).ToString()!;

                UpdateCells(changedCells);
            }
            catch (Exception exception)
            {
                //Handle exception (display error message)
                cc_ContentText = "";
                CellsBackingStore[cc_CellRow, cc_CellCol] = "";
                hasError = true;
                errorText = exception;
            }

            StateHasChanged();
        }
    }

    /// <summary>
    ///     Updates all cell contents of the given list of cells (dependent cells)
    /// </summary>
    /// <param name="changedCells"> List of cells to update </param>
    private void UpdateCells(IList<string> changedCells)
    {
        foreach (var name in changedCells)
        {
            int[] rowcol = ConvertCellToRowCol(name);
            CellsBackingStore[rowcol[0], rowcol[1]] = sheet.GetCellValue(name).ToString()!;
            CellsColorStore[rowcol[0], rowcol[1]] = sheet.GetCellColor(name);
        }
    }

    /// <summary>
    ///     Converts the given cell name to it's coordinate on the spreadsheet grid
    /// </summary>
    /// <param name="name"> name of cell </param>
    /// <returns> An int array of [row, col] </returns>
    private int[] ConvertCellToRowCol(string name)
    {
        int[] rowcol = new int[2];
        char[] nameChars = name.ToCharArray(); // ex. 'A' '1'

        rowcol[0] = (int)char.GetNumericValue(nameChars[1]) - 1;
        rowcol[1] = Alphabet.ToList<char>().IndexOf(nameChars[0]);

        return rowcol;
    }

    /// <summary>
    ///     Used to close the text box after an error is displayed
    /// </summary>
    private void DismissError()
    {
        hasError = false;
    }


    /// <summary>
    /// Saves the current spreadsheet, by providing a download of a file
    /// containing the json representation of the spreadsheet.
    /// </summary>
    private async void SaveFile()
    {
        if (sheet.Changed)
            await JSRuntime.InvokeVoidAsync("downloadFile", FileSaveName, sheet.GetJsonString());
    }

    /// <summary>
    /// This method will run when the file chooser is used, for loading a file.
    /// Uploads a file containing a json representation of a spreadsheet, and 
    /// replaces the current sheet with the loaded one.
    /// </summary>
    /// <param name="args">The event arguments, which contains the selected file name</param>
    private async void HandleFileChooser( EventArgs args )
    {
        try
        {
            string fileContent = string.Empty;

            InputFileChangeEventArgs eventArgs = args as InputFileChangeEventArgs ?? throw new Exception("unable to get file name");
            if ( eventArgs.FileCount == 1 )
            {
                var file = eventArgs.File;
                if ( file is null )
                {
                    return;
                }

                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);

                // fileContent will contain the contents of the loaded file
                fileContent = await reader.ReadToEndAsync();

                var changed = sheet.SetSpreadsheetFromJson(fileContent);

                UpdateCells(changed);

                StateHasChanged();
            }
        }
        catch ( Exception e )
        {
            Debug.WriteLine( "an error occurred while loading the file..." + e );
        }
    }

}
