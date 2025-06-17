// <authors> Logan Wood </authors>
// <date> Sept. 27, 2024 </date>


namespace SpreadsheetTests;

using CS3500.Formula;
using CS3500.Spreadsheet;
using System.Diagnostics;

/// <summary>
/// The following class is used for testing the Spreadsheet.cs project and its
/// functionality as required by PS5.
/// </summary>
[TestClass]
public class SpreadsheetTests
{

    // TESTS FOR SetCellContents

    [TestMethod]
    public void Test_SetCellContents_WithNumber()
    {
        Spreadsheet sheet = new Spreadsheet();

        // Act
        var changedCells = sheet.SetContentsOfCell("A1", "5.0");

        // Assert
        Assert.AreEqual(5.0, sheet.GetCellContents("A1"));
        Assert.IsTrue(changedCells.Contains("A1"));
        Assert.AreEqual(1, changedCells.Count);
    }

    [TestMethod]
    public void Test_SetCellContents_WithNumber_ReplaceContents()
    {
        Spreadsheet sheet = new Spreadsheet();

        // Act
        var changedCells = sheet.SetContentsOfCell("A1", "Hello"); //Initially, A1 contains "hello"
        changedCells = sheet.SetContentsOfCell("A1", "5.0"); //Now, A1 contains 5.0
        // Assert
        Assert.AreEqual(5.0, sheet.GetCellContents("A1"));
        Assert.IsTrue(changedCells.Contains("A1"));
        Assert.AreEqual(1, changedCells.Count);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void Test_SetCellContents_WithNumber_InvalidName()
    {
        Spreadsheet sheet = new Spreadsheet();

        var changedCells = sheet.SetContentsOfCell("C 2", "30.0");
    }

    [TestMethod]
    public void Test_SetCellContents_WithText()
    {
        Spreadsheet sheet = new Spreadsheet();

        // Act
        var changedCells = sheet.SetContentsOfCell("B1", "Hello");

        // Assert
        Assert.AreEqual("Hello", sheet.GetCellContents("B1"));
        Assert.IsTrue(changedCells.Contains("B1"));
        Assert.AreEqual(1, changedCells.Count);
    }

    [TestMethod]
    public void Test_SetCellContents_WithText_ReplaceContents()
    {
        Spreadsheet sheet = new Spreadsheet();

        // Act
        var changedCells = sheet.SetContentsOfCell("A1", "40.0"); //Initially, A1 contains "hello"
        changedCells = sheet.SetContentsOfCell("A1", "Hello"); //Now, A1 contains 5.0
        // Assert
        Assert.AreEqual("Hello", sheet.GetCellContents("A1"));
        Assert.IsTrue(changedCells.Contains("A1"));
        Assert.AreEqual(1, changedCells.Count);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void Test_SetCellContents_WithText_InvalidName()
    {
        Spreadsheet sheet = new Spreadsheet();

        var changedCells = sheet.SetContentsOfCell("C2!", "hello");
    }

    [TestMethod]
    public void Test_SetCellContents_WithFormula()
    {
        Spreadsheet sheet = new Spreadsheet();
        Formula f = new Formula("A1 + 2");

        // Act
        var changedCells = sheet.SetContentsOfCell("C1", "=A1 + 2");

        // Assert
        Assert.IsTrue(changedCells.Contains("C1"));
        Assert.AreEqual(f, sheet.GetCellContents("C1"));
    }

    [TestMethod]
    public void Test_SetCellContents_WithFormula_ReplaceContents()
    {
        Spreadsheet sheet = new Spreadsheet();

        // Act
        var changedCells = sheet.SetContentsOfCell("A1", "Hello"); //Initially, A1 contains "hello"
        changedCells = sheet.SetContentsOfCell("A1", "=1 - 6 + 4"); //Now, A1 contains 5.0
        // Assert
        Assert.AreEqual(new Formula("1 - 6 + 4"), sheet.GetCellContents("A1"));
        Assert.IsTrue(changedCells.Contains("A1"));
        Assert.AreEqual(1, changedCells.Count);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void Test_SetCellContents_WithFormula_InvalidName()
    {
        Spreadsheet sheet = new Spreadsheet();

        var changedCells = sheet.SetContentsOfCell("", "=A1 + 2");
    }

    // TESTS FOR GetCellContents

    [TestMethod]
    public void Test_GetCellContents_Empty()
    {
        Spreadsheet sheet = new Spreadsheet();

        // Assert
        Assert.AreEqual(string.Empty, sheet.GetCellContents("A1"));
    }

    [TestMethod]
    public void Test_GetCellContents_IsNumber()
    {
        Spreadsheet sheet = new Spreadsheet();

        sheet.SetContentsOfCell("A1", "3.0");

        // Assert
        Assert.AreEqual(3.0, sheet.GetCellContents("A1"));
    }

    [TestMethod]
    public void Test_GetCellContents_IsText()
    {
        Spreadsheet sheet = new Spreadsheet();

        sheet.SetContentsOfCell("A1", "Hello");

        // Assert
        Assert.AreEqual("Hello", sheet.GetCellContents("A1"));
    }

    [TestMethod]
    public void Test_GetCellContents_IsFormula()
    {
        Spreadsheet sheet = new Spreadsheet();

        sheet.SetContentsOfCell("A1", "=1 - 3 * (1-7)");

        // Assert
        Assert.AreEqual(new Formula("1 - 3 * (1 - 7)"), sheet.GetCellContents("A1"));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void Test_GetCellContents_InvalidName()
    {
        Spreadsheet sheet = new Spreadsheet();

        sheet.GetCellContents(""); //Empty cell name = invalid cell name
    }

    [TestMethod]
    [ExpectedException(typeof(CircularException))]
    public void Test_SetCellContents_CircularDependency_Throw()
    {
        Spreadsheet sheet = new Spreadsheet();

        sheet.SetContentsOfCell("A1", "=B1 + 2");
        sheet.SetContentsOfCell("B1", "=C1 + 2");

        sheet.SetContentsOfCell("C1", "=A1 + 2");

    }

    [TestMethod]
    public void Test_SetCellContents_UpdateToEmpty_Valid()
    {
        Spreadsheet sheet = new Spreadsheet();

        sheet.SetContentsOfCell("A1", "5");

        Assert.IsTrue(sheet.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());

        sheet.SetContentsOfCell("A1", "");

        //"A1" should now have been removed from the list of nonempty cells since it was changed to empty
        Assert.IsFalse(sheet.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
    }

    //TESTS FOR GetNamesOfAllNonemptyCells

    [TestMethod]
    public void Test_GetNamesOfAllNonemptyCells_MultipleCells()
    {
        Spreadsheet sheet = new Spreadsheet();

        sheet.SetContentsOfCell("A1", "=1 - 3 * (1-7)");
        sheet.SetContentsOfCell("B2", "hello!");
        sheet.SetContentsOfCell("C3", "4.0");

        var names = sheet.GetNamesOfAllNonemptyCells();

        // Assert
        Assert.IsTrue(names.Contains("A1"));
        Assert.IsTrue(names.Contains("B2"));
        Assert.IsTrue(names.Contains("C3"));
    }

    // TESTS FOR GetCellValue

    [TestMethod]
    public void Test_GetCellValue_FormulaNoVar_Valid()
    {
        Spreadsheet sheet = new Spreadsheet();

        sheet.SetContentsOfCell("A1", "=4 + 5"); // Value should evaluate to 9

        Assert.AreEqual(9.0, sheet.GetCellValue("A1"));
    }

    [TestMethod]
    public void Test_GetCellValue_EmptyCell_Valid()
    {
        Spreadsheet sheet = new Spreadsheet();

        Assert.AreEqual("", sheet.GetCellValue("A1"));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void Test_GetCellValue_InvalidCellName_Invalid()
    {
        Spreadsheet sheet = new Spreadsheet();

        sheet.GetCellValue("AB4F"); //Invalid name
    }

    // TESTS FOR Spreadsheet[name] <-- way of getting cell value

    [TestMethod]
    public void Test_ThisCellValue_FormulaNoVar_Valid()
    {
        Spreadsheet sheet = new Spreadsheet();

        sheet.SetContentsOfCell("A1", "=4 + 5"); // Value should evaluate to 9

        Assert.AreEqual(9.0, sheet["A1"]);
    }

    [TestMethod]
    public void Test_ThisCellValue_EmptyCell_Valid()
    {
        Spreadsheet sheet = new Spreadsheet();

        Assert.AreEqual("", sheet["A1"]);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void Test_ThisCellValue_InvalidCellName_Invalid()
    {
        Spreadsheet sheet = new Spreadsheet();

        sheet.SetContentsOfCell("A1", "=4 + 5"); // Value should evaluate to 9
        var a = sheet["AB3F"];
    }


    // TESTS FOR Save

    [TestMethod]
    public void Test_Save_Simple_Valid()
    {
        Spreadsheet sheet = new Spreadsheet();

        //sheet.SetContentsOfCell("A1", "=A2+B3");
        //sheet.SetContentsOfCell("A2", "3.0");
        //sheet.SetContentsOfCell("B3", "=4/2");
        //sheet.SetContentsOfCell("C1", "Hello");
        sheet.SetContentsOfCell("A1", "5");
        sheet.SetContentsOfCell("B3", "=A1+2");
        sheet.SetContentsOfCell("C4", "hello");

        sheet.Save("\\Users\\Logan\\Desktop\\beans\\TestSpreadsheet.txt");
    }

    [TestMethod]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void Test_Save_NonExistentPath_Invalid()
    {
        Spreadsheet sheet = new Spreadsheet();

        sheet.SetContentsOfCell("A1", "=A2+B3");
        sheet.SetContentsOfCell("A2", "3.0");
        sheet.SetContentsOfCell("B3", "=4/2");
        sheet.SetContentsOfCell("C1", "Hello");

        sheet.Save("/some/nonsense/path.txt");
    }

    // TESTS FOR Spreadsheet load file constructor 
    
    [TestMethod]
    public void Test_SpreadsheetLoad_Simple_Valid()
    {
        Spreadsheet sheet = new Spreadsheet();

        sheet.SetContentsOfCell("A1", "5");
        sheet.SetContentsOfCell("B3", "=A1+2");
        sheet.SetContentsOfCell("C4", "hello");

        sheet.Save("TestSpreadsheet.txt");

        Spreadsheet sheet2 = new Spreadsheet("TestSpreadsheet.txt");

        Assert.AreEqual(5.0, sheet2.GetCellContents("A1"));
        Assert.AreEqual(new Formula("A1+2"), sheet2.GetCellContents("B3"));
        Assert.AreEqual("hello", sheet2.GetCellContents("C4"));
    }

    [TestMethod]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void Test_SpreadsheetLoad_NonExistentFile_Invalid()
    {
        Spreadsheet sheet = new Spreadsheet();

        sheet.SetContentsOfCell("A1", "5");
        sheet.SetContentsOfCell("B3", "=A1+2");
        sheet.SetContentsOfCell("C4", "hello");

        sheet.Save("TestSpreadsheet.txt");

        Spreadsheet sheet2 = new Spreadsheet("spaghetti.txt");
    }

    [TestMethod]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void Test_SpreadsheetLoad_NonExistentPath_Invalid()
    {
        Spreadsheet sheet = new Spreadsheet();

        sheet.SetContentsOfCell("A1", "5");
        sheet.SetContentsOfCell("B3", "=A1+2");
        sheet.SetContentsOfCell("C4", "hello");

        sheet.Save("TestSpreadsheet.txt");

        Spreadsheet sheet2 = new Spreadsheet("/some/nonsense/path.txt");
    }

}