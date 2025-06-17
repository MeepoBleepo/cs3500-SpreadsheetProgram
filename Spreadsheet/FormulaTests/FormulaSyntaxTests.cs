// <copyright file="FormulaSyntaxTests.cs" company="UofU-CS3500">
//   Copyright © 2024 UofU-CS3500. All rights reserved.
// </copyright>
// <authors> Logan Wood </authors>
// <date> Aug. 29, 2024 </date>

namespace FormulaTests;

using CS3500.Formula;

/// <summary>
///   <para>
///     The following class shows the basics of how to use the MSTest framework,
///     including:
///   </para>
///   <list type="number">
///     <item> How to catch exceptions. </item>
///     <item> How a test of valid code should look. </item>
///   </list>
/// </summary>
[TestClass]
public class FormulaSyntaxTests
{
    // --- Tests for One Token Rule ---
    #region Tests for One Token Rule

    /// <summary>
    ///   <para>
    ///     This test makes sure the right kind of exception is thrown
    ///     when trying to create a formula with no tokens.
    ///   </para>
    ///   <remarks>
    ///     <list type="bullet">
    ///       <item>
    ///         We use the _ (discard) notation because the formula object
    ///         is not used after that point in the method.  Note: you can also
    ///         use _ when a method must match an interface but does not use
    ///         some of the required arguments to that method.
    ///       </item>
    ///       <item>
    ///         string.Empty is often considered best practice (rather than using "") because it
    ///         is explicit in intent (e.g., perhaps the coder forgot to but something in "").
    ///       </item>
    ///       <item>
    ///         The name of a test method should follow the MS standard:
    ///         https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
    ///       </item>
    ///       <item>
    ///         All methods should be documented, but perhaps not to the same extent
    ///         as this one.  The remarks here are for your educational
    ///         purposes (i.e., a developer would assume another developer would know these
    ///         items) and would be superfluous in your code.
    ///       </item>
    ///       <item>
    ///         Notice the use of the attribute tag [ExpectedException] which tells the test
    ///         that the code should throw an exception, and if it doesn't an error has occurred;
    ///         i.e., the correct implementation of the constructor should result
    ///         in this exception being thrown based on the given poorly formed formula.
    ///       </item>
    ///     </list>
    ///   </remarks>
    ///   <example>
    ///     <code>
    ///        // here is how we call the formula constructor with a string representing the formula
    ///        _ = new Formula( "5+5" );
    ///     </code>
    ///   </example>
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void FormulaConstructor_TestNoTokens_Invalid()
    {
        _ = new Formula("");  // note: it is arguable that you should replace "" with string.Empty for readability and clarity of intent (e.g., not a cut and paste error or a "I forgot to put something there" error).
    }

    /// <summary>
    /// This test makes sure that no exceptions are thrown when creating a formula
    /// with only one token.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestOneToken_Valid()
    {
        _ = new Formula("1");
    }

    #endregion

    // --- Tests for Valid Token Rule ---
    #region Tests for Valid Token Rule

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void FormulaConstructor_TestInvalidOperatorToken_Invalid()
    {
        _ = new Formula("2>1");
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void FormulaConstructor_TestInvalidToken_Invalid()
    {
        _ = new Formula("5# - 2");
    }

    [TestMethod]
    public void FormulaConstructor_TestValidOperatorTokenPlus_Valid()
    {
        _ = new Formula("1+1");
    }
    [TestMethod]
    public void FormulaConstructor_TestValidTokenMinus_Valid()
    {
        _ = new Formula("1-1");
    }
    [TestMethod]
    public void FormulaConstructor_TestValidTokenAsterisk_Valid()
    {
        _ = new Formula("1*1");
    }
    [TestMethod]
    public void FormulaConstructor_TestValidTokenSlash_Valid()
    {
        _ = new Formula("1/1");
    }
    [TestMethod]
    public void FormulaConstructor_TestValidTokenVariable_Valid()
    {
        _ = new Formula("a1");
    }
    [TestMethod]
    public void FormulaConstructor_TestValidTokenVariableUppercase_Valid()
    {
        _ = new Formula("A1");
    }
    [TestMethod]
    public void FormulaConstructor_TestValidTokenExponent_Valid()
    {
        _ = new Formula("2E5");
    }
    [TestMethod]
    public void FormulaConstructor_TestValidTokenDecimal_Valid()
    {
        _ = new Formula("3.14");
    }
    [TestMethod]
    public void FormulaConstructor_TestValidTokenNegativeExponent_Valid()
    {
        _ = new Formula("3.5E-6");
    }

    #endregion

    // --- Tests for Closing Parentheses Rule
    #region Tests for Closing Parentheses Rule

    [TestMethod]
    public void FormulaConstructor_TestClosingParentheses_Valid()
    {
        // Valid: Correct balance of closing and opening parentheses
        _ = new Formula("((3 + 5) * 2)");
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void FormulaConstructor_TestExtraClosingParentheses_Invalid()
    {
        // Invalid: More closing parentheses than opening at one point
        _ = new Formula("(1+1))");
    }

    #endregion

    // --- Tests for Balanced Parentheses Rule
    #region Tests for Balanced Parentheses Rule

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void FormulaConstructor_TestUnbalancedParenthesis_Invalid()
    {
        // Invalid: More opening parentheses than closing
        _ = new Formula("(1+1");
    }

    [TestMethod]
    public void FormulaConstructor_TestBalancedParentheses_Valid()
    {
        // Valid: Equal number of opening and closing parentheses
        _ = new Formula("(1+1)");
    }

    /// <summary>
    /// Tests a valid scenario of the Balanced Parenthesis Rule for a scenario in which a formula has 2 groups of parenthesis pairs
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestBalancedMultiParentheses_Valid()
    {
        _ = new Formula("(1+1)+(1+1)");
    }

    #endregion

    // --- Tests for First Token Rule
    #region Tests for First Token Rule

    [TestMethod]
    public void FormulaConstructor_TestFirstTokenNumber_Valid()
    {
        _ = new Formula("1 + 1");
    }

    [TestMethod]
    public void FormulaConstructor_TestFirstTokenParentheses_Valid()
    {
        _ = new Formula("(5 + 3)");
    }

    [TestMethod]
    public void FormulaConstructor_TestFirstTokenVariable_Valid()
    {
        _ = new Formula("x1 + 3");
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void FormulaConstructor_TestFirstTokenOperator_Invalid()
    {
        _ = new Formula("+ 1 * 5");
    }

    #endregion

    // --- Tests for  Last Token Rule ---
    #region Tests for  Last Token Rule 

    [TestMethod]
    public void FormulaConstructor_TestLastTokenNumber_Valid()
    {
        _ = new Formula("1 + 1");
    }

    [TestMethod]
    public void FormulaConstructor_TestLastTokenParenthesis_Valid()
    {
        _ = new Formula("(1 + 1)");
    }

    [TestMethod]
    public void FormulaConstructor_TestLastTokenVariable_Valid()
    {
        _ = new Formula("5 + x1");
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void FormulaConstructor_TestLastTokenOperator_Invalid()
    {
        _ = new Formula("3 + 5 *");
    }

    #endregion

    // --- Tests for Parentheses/Operator Following Rule ---
    #region Tests for Parentheses/Operator Following Rule

    [TestMethod]
    public void FormulaConstructor_TestNumberFollowingParentheses_Valid()
    {
        _ = new Formula("(5 + 3)");
    }

    [TestMethod]
    public void FormulaConstructor_TestVariableFollowingParentheses_Valid()
    {
        _ = new Formula("(x1 + 3)");
    }

    [TestMethod]
    public void FormulaConstructor_TestParenthesesFollowingOperator_Valid()
    {
        _ = new Formula("1 + (2 + 1)");
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void FormulaConstructor_TestParenthesesFollowingParentheses_Invalid()
    {
        _ = new Formula("()");
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void FormulaConstructor_TestOperatorFollowingParentheses_Invalid()
    {
        _ = new Formula("(+ 3)");
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void FormulaConstructor_TestOperatorFollowingOperator_Invalid()
    {
        _ = new Formula("1 ++ 1");
    }

    #endregion

    // --- Tests for Extra Following Rule ---
    #region Tests for Extra Following Rule

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void FormulaConstructor_TestOpenParenthesesFollowingNumber_Invalid()
    {
        _ = new Formula("1(1+1)");
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void FormulaConstructor_TestVariableFollowingNumber_Invalid()
    {
        _ = new Formula("1x3");
    }

    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void FormulaConstructor_TestNumberFollowingNumber_Invalid()
    {
        _ = new Formula("5 3 + 2");
    }

    #endregion

    [TestMethod]
    public void FormulaGetVariables_TestGetVariablesContains_Valid()
    {
         var f = new Formula("a1 + (B2 + A5) * A1");
        HashSet<string> varTokens = (HashSet<string>)f.GetVariables();
        Assert.IsTrue(varTokens.Contains("A1"));
        Assert.IsTrue(varTokens.Contains("B2"));
        Assert.IsTrue(varTokens.Contains("A5"));
    }

    [TestMethod]
    public void FormulaGetVariables_TestGetVariablesNoDuplicates_Valid()
    {
        var f = new Formula("a1 + A1");
        HashSet<string> varTokens = (HashSet<string>)f.GetVariables();
        Assert.IsTrue(varTokens.Contains("A1"));
        Assert.IsFalse(varTokens.Contains("a1"));
    }

    [TestMethod]
    public void FormulaToString_TestNumbersAndVariable_Valid()
    {
        var f = new Formula("33.000 + 4E4+a7");
        Assert.IsTrue(f.ToString().Equals("33+40000+A7"));
    }

}