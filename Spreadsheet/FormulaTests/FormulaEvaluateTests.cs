// <copyright file="FormulaSyntaxTests.cs" company="UofU-CS3500">
//   Copyright © 2024 UofU-CS3500. All rights reserved.
// </copyright>
// <authors> Logan Wood </authors>
// <date> September 18, 2024 </date>

namespace FormulaTests;

using CS3500.Formula;

/// <summary>
/// The following class is used for testing the Evaluate method as well as 
/// other methods added to Formula.cs in PS4
/// </summary>
[TestClass]
public class FormulaEvaluateTests
{

    [TestMethod]
    public void TestFormulaEvaluate_SimpleFormula_Valid()
    {
        Formula f = new Formula("1 + 3");
        Assert.AreEqual(4.0, f.Evaluate(s => 0));
    }

    [TestMethod]
    public void TestFormulaEvaluate_SimpleFormula2()
    {
        Formula f = new Formula("1 - 1 - 4");
        Assert.AreEqual(-4.0, f.Evaluate(s => 0));
    }

    [TestMethod]
    public void TestFormulaEvaluate_SimpleFormula3()
    {
        Formula f = new Formula("1 * 1 - (4 * 1) / (4 / 2) * (1 - 2)"); 
        Assert.AreEqual(3.0, f.Evaluate(s => 0));
    }

    [TestMethod]
    public void TestFormulaEvaluate_DivideByZero_FormulaError()
    {
        Formula f = new Formula("1 / 0");
        Assert.IsInstanceOfType(f.Evaluate(s => 0), typeof(FormulaError));
    }

    [TestMethod]
    public void TestFormulaEvaluate_DivideByZeroOtherDirection_FormulaError()
    {
        Formula f = new Formula("0 / 1");
        Assert.IsInstanceOfType(f.Evaluate(s => 0), typeof(FormulaError));
    }

    [TestMethod]
    public void TestFormulaEvaluate_DivideByZeroParentheses_FormulaError()
    {
        Formula f = new Formula("1 / (3 - 3)");
        Assert.IsInstanceOfType(f.Evaluate(s => 0), typeof(FormulaError));
    }

    [TestMethod]
    public void TestFormulaEvaluate_UnknownVariable_FormulaError()
    {
        Formula f = new Formula("A1 / 5");
        Assert.IsInstanceOfType(f.Evaluate(s => 
        {
            if (s.Equals("A2")) //Since there is no lookup for A1 in here, it will throw ArgumentException
                return 0;
            else
                throw new ArgumentException();     
        }), typeof(FormulaError)); 
    }

    [TestMethod]
    public void TestFormulaEvaluate_KnownVariables_Valid()
    {
        Formula f = new Formula("A1 - B3");
        Assert.AreEqual(-1.0, f.Evaluate(s =>
        {
            if (s.Equals("A1"))
                return 3;
            else if (s.Equals("B3"))
                return 4;
            else
                throw new ArgumentException();
        }));
    }

    [TestMethod]
    public void TestFormulaEvaluate_MultipleOperators_Valid()
    {
        Formula f = new Formula("3 + 1 / 4 * (2 + 2) - 1");
        Assert.AreEqual(3.0, f.Evaluate(s => 0));
    }

    [TestMethod]
    public void TestFormulaEvaluate_TwoOperators_Valid()
    {
        Formula f = new Formula("3.5 + 1 / 4");
        Assert.AreEqual(3.75, f.Evaluate(s => 0));
    }

    [TestMethod]
    public void TestFormulaEvaluate_SimpleDecimals_Valid()
    {
        Formula f = new Formula("3.5 + 4.25");
        Assert.AreEqual(7.75, f.Evaluate(s => 0));
    }

    // TESTS FOR Equals, ==, and !=

    [TestMethod]
    public void TestFormulaEqualsOperator_TrueCase()
    {
        Formula f1 = new Formula("4 + 7 / 5 - (4 + 3)");
        Formula f2 = new Formula("4+7/5-(4+3)");
        Assert.IsTrue(f1 == f2);
    }

    [TestMethod]
    public void TestFormulaEqualsOperator_FalseCase()
    {
        Formula f1 = new Formula("3 + 7 / 5 - (4 + 3)");
        Formula f2 = new Formula("4+7/5-(4+3)");
        Assert.IsFalse(f1 == f2);
    }

    [TestMethod]
    public void TestFormulaNotEqualsOperator_TrueCase()
    {
        Formula f1 = new Formula("4 + 7 / 5 - (4.2 + 3)");
        Formula f2 = new Formula("4+7/5-(4+3)");
        Assert.IsTrue(f1 != f2);
    }

    [TestMethod]
    public void TestFormulaNotEqualsOperator_FalseCase()
    {
        Formula f1 = new Formula("4 + 7 / 5 - (4 + 3)");
        Formula f2 = new Formula("4+7/5-(4+3)");
        Assert.IsFalse(f1 != f2);
    }

    [TestMethod]
    public void TestFormulaEquals_TrueCase()
    {
        Formula f1 = new Formula("3.5 + 4.25");
        Formula f2 = new Formula("3.5+4.25");
        Assert.IsTrue(f1.Equals(f2));
    }

    [TestMethod]
    public void TestFormulaEquals_FalseCaseNull()
    {
        Formula f1 = new Formula("3.5 + 4.25");
        Formula f2 = null;
        Assert.IsFalse(f1.Equals(f2));
    }

    [TestMethod]
    public void TestFormulaEquals_FalseCaseDiffObject()
    {
        Formula f1 = new Formula("3.5 + 4.25");
        Object f2 = new object();
        Assert.IsFalse(f1.Equals(f2));
    }

    //TESTS FOR GetHashcode()

    [TestMethod]
    public void TestFormulaGetHashCode_EqualCodes()
    {
        Formula f1 = new Formula("3.5 + 4.25");
        Formula f2 = new Formula("3.5+4.25");
        Assert.IsTrue(f1.Equals(f2));

        Assert.IsTrue(f1.GetHashCode().Equals(f2.GetHashCode()));
    }

    [TestMethod]
    public void TestFormulaGetHashCode_SameValueDifferentFormula()
    {
        Formula f1 = new Formula("3.5 + 4.25");     // = 7.75
        Formula f2 = new Formula("3 + 4.0 + 0.75"); // = 7.75
        Assert.IsFalse(f1.Equals(f2));

        Assert.IsFalse(f1.GetHashCode().Equals(f2.GetHashCode()));
    }
}
