// <copyright file="Formula_PS2.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>
// <summary>
//   <para>
//     This code is provides to start your assignment.  It was written
//     by Profs Joe, Danny, and Jim.  You should keep this attribution
//     at the top of your code where you have your header comment, along
//     with the other required information.
//   </para>
//   <para>
//     You should remove/add/adjust comments in your file as appropriate
//     to represent your work and any changes you make.
//   </para>
// </summary>

// Complete implementation written by Logan Wood for CS 3500
// Version for PS2 written September 6, 2024
// Version for PS4 written September 20, 2024


namespace CS3500.Formula;

using System.Text.RegularExpressions;

/// <summary>
///   <para>
///     This class represents formulas written in standard infix notation using standard precedence
///     rules.  The allowed symbols are non-negative numbers written using double-precision
///     floating-point syntax; variables that consist of one ore more letters followed by
///     one or more numbers; parentheses; and the four operator symbols +, -, *, and /.
///   </para>
///   <para>
///     Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
///     a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable;
///     and "x 23" consists of a variable "x" and a number "23".  Otherwise, spaces are to be removed.
///   </para>
///   <para>
///     For Assignment Two, you are to implement the following functionality:
///   </para>
///   <list type="bullet">
///     <item>
///        Formula Constructor which checks the syntax of a formula.
///     </item>
///     <item>
///        Get Variables
///     </item>
///     <item>
///        ToString
///     </item>
///   </list>
/// </summary>
public class Formula
{
    /// <summary> A list holding each token split by GetTokens method called in the constructor.</summary>
    private List<string> tokens;

    /// <summary> Holds the normalized/canonical version of this object's 
    /// formula. Is built during object construction </summary>
    private string normalizedFormula = "";

    /// <summary>
    ///   All variables are letters followed by numbers.  This pattern
    ///   represents valid variable name strings.
    /// </summary>
    private const string VariableRegExPattern = @"[a-zA-Z]+\d+";
    /// <summary>This pattern represents valid opening parentheses strings.</summary>
    private const string LpPattern = @"\(";
    /// <summary>This pattern represents valid closing parentheses strings.</summary>
    private const string RpPattern = @"\)";
    /// <summary>This pattern represents valid operator symbol strings.</summary>
    private const string OpPattern = @"[\+\-*/]";
    /// <summary>Numbers can be integers, contain decimals, or be written in scientific notation. 
    /// This pattern represents valid number strings.</summary>
    private const string DoublePattern = @"(?:\d+\.\d*|\d*\.\d+|\d+)(?:[eE][\+-]?\d+)?";

    /// <summary>
    ///   Initializes a new instance of the <see cref="Formula"/> class.
    ///   <para>
    ///     Creates a Formula from a string that consists of an infix expression written as
    ///     described in the class comment.  If the expression is syntactically incorrect,
    ///     throws a FormulaFormatException with an explanatory Message.  See the assignment
    ///     specifications for the syntax rules you are to implement.
    ///   </para>
    ///   <para>
    ///     Non Exhaustive Example Errors:
    ///   </para>
    ///   <list type="bullet">
    ///     <item>
    ///        Invalid variable name, e.g., x, x1x  (Note: x1 is valid, but would be normalized to X1)
    ///     </item>
    ///     <item>
    ///        Empty formula, e.g., string.Empty
    ///     </item>
    ///     <item>
    ///        Mismatched Parentheses, e.g., "(("
    ///     </item>
    ///     <item>
    ///        Invalid Following Rule, e.g., "2x+5"
    ///     </item>
    ///   </list>
    /// </summary>
    /// <param name="formula"> The string representation of the formula to be created.</param>
    public Formula(string formula)
    {
        tokens = GetTokens(formula);

        //One Token Rule
        if(tokens.Count == 0)
            throw new FormulaFormatException("Formula must contain at least one token.");
        else
        {
            //First Token Rule
            if (!(IsVar(tokens[0]) || IsNum(tokens[0]) || IsLP(tokens[0])))
                throw new FormulaFormatException(tokens[0] + " is not a valid first token. Violates First Token Rule.");
            //Last Token Rule
            if (!(IsVar(tokens[tokens.Count-1]) || IsNum(tokens[tokens.Count - 1]) || IsRP(tokens[tokens.Count - 1])))
                throw new FormulaFormatException(tokens[0] + " is not a valid last token. Violates Last Token Rule.");
        }

        int lParenCount = 0; //Left/Open Parentheses count
        int rParenCount = 0; //Right/Closed Parentheses count
        string lastToken = null; //Previous token
        foreach (String token in tokens)
        {
            //Normalize and add this token to the normalizedFormula string
            ConcatenateCanonicalToken(token);

            //Valid Token Rule
            if (!(IsVar(token) || IsNum(token) || IsOp(token) || IsLP(token) || IsRP(token)))
                throw new FormulaFormatException(token + " is not a valid token.");

            //Closing Parentheses Rule & Update parentheses count
            if(IsLP(token)) 
                lParenCount++;
            else if (IsRP(token))
            {
                rParenCount++;
                if (rParenCount > lParenCount)
                    throw new FormulaFormatException("Amount of closed parenthesis is greater than amount of open parenthesis.");
            }

            //Parenthesis/Operator Following Rule
            if (lastToken != null && (IsLP(lastToken) || IsOp(lastToken)))
                if (!(IsNum(token) || IsVar(token) || IsLP(token)))
                    throw new FormulaFormatException("\"" + token + "\"" + " was detected without a valid token after it.");

            //Extra Following Rule
            if (lastToken != null && (IsNum(lastToken) || IsVar(lastToken) || IsRP(lastToken)))
                if (!(IsOp(token) || IsRP(token)))
                    throw new FormulaFormatException("\"" + token + "\"" + " was detected without a valid token after it.");

            //Update the value of lastToken
            lastToken = token;
        }
        
        //Balanced Parentheses Rule
        if (lParenCount != rParenCount)
            throw new FormulaFormatException("Formula contains imbalance with number of opening and closing parentheses.");

    }

    /// <summary>
    ///   <para>
    ///     Returns a set of all the variables in the formula.
    ///   </para>
    ///   <remarks>
    ///     Important: no variable may appear more than once in the returned set, even
    ///     if it is used more than once in the Formula.
	///     Variables should be returned in canonical form, having all letters converted
	///     to uppercase.
    ///   </remarks>
    ///   <list type="bullet">
    ///     <item>new("x1+y1*z1").GetVariables() should return a set containing "X1", "Y1", and "Z1".</item>
    ///     <item>new("x1+X1"   ).GetVariables() should return a set containing "X1".</item>
    ///   </list>
    /// </summary>
    /// <returns> the set of variables (string names) representing the variables referenced by the formula. </returns>
    public ISet<string> GetVariables()
    {
        var varTokens = new HashSet<string>();
        foreach (string token in tokens)
            if(IsVar(token))
                varTokens.Add(token.ToUpper());
        
        return varTokens;
    }

    /// <summary>
    ///   <para>
    ///     Returns a string representation of a canonical form of the formula.
    ///   </para>
    ///   <para>
    ///     The string will contain no spaces.
    ///   </para>
    ///   <para>
    ///     If the string is passed to the Formula constructor, the new Formula f 
    ///     will be such that this.ToString() == f.ToString().
    ///   </para>
    ///   <para>
    ///     All of the variables in the string will be normalized.  This
    ///     means capital letters.
    ///   </para>
    ///   <para>
    ///       For example:
    ///   </para>
    ///   <code>
    ///       new("x1 + y1").ToString() should return "X1+Y1"
    ///       new("X1 + 5.0000").ToString() should return "X1+5".
    ///   </code>
    ///   <para>
    ///     This code should execute in O(1) time.
    ///   <para>
    /// </summary>
    /// <returns>
    ///   A canonical version (string) of the formula. All "equal" formulas
    ///   should have the same value here.
    /// </returns>
    public override string ToString()
    {
        return normalizedFormula;
    }

    /// <summary>
    /// Private helper method which converts the provided token to canonical form and 
    /// concatenates it to this object's private normalizedFormula string.
    /// </summary>
    /// <param name="token"></param>
    private void ConcatenateCanonicalToken(string token)
    {
        token = token.ToUpper();

        //Normalization for number tokens
        double parsedNum;
        if (Double.TryParse(token, out parsedNum))
            token = parsedNum.ToString();

        //Concatenate to canonicalFormula string
        normalizedFormula += token;
    }

    /// <summary>
    ///   Reports whether "token" is a variable.  It must be one or more letters
    ///   followed by one or more numbers.
    /// </summary>
    /// <param name="token"> A token that may be a variable. </param>
    /// <returns> true if the string matches the requirements, e.g., A1 or a1. </returns>
    private static bool IsVar(string token)
    {
        // notice the use of ^ and $ to denote that the entire string being matched is just the variable
        string standaloneVarPattern = $"^{VariableRegExPattern}$";
        return Regex.IsMatch(token, standaloneVarPattern);
    }

    /// <summary>
    ///   Reports whether "token" is a number. Numbers an be contain decimals, 
    ///   and can be written in scientific notation
    /// </summary>
    /// <param name="token"> A token that may be a number. </param>
    /// <returns> true if the string matches the requirements, e.g., 3, 3.14, or 3E4. </returns>
    private static bool IsNum(string token)
    {
        string standaloneNumPattern = $"^{DoublePattern}$";
        return Regex.IsMatch(token, standaloneNumPattern);
    }

    /// <summary>
    ///   Reports whether "token" is an operator. Valid formula operators
    ///   are "+", "-", "*", or "/".
    /// </summary>
    /// <param name="token"> A token that may be an operator. </param>
    /// <returns> true if the string matches the requirements, e.g., + or *. </returns>
    private static bool IsOp(string token)
    {
        string standaloneOpPattern = $"^{OpPattern}$";
        return Regex.IsMatch(token, standaloneOpPattern);
    }

    /// <summary>
    ///   Reports whether "token" is a left parenthesis (opening parenthesis). 
    /// </summary>
    /// <param name="token"> A token that may be an opening parenthesis. </param>
    /// <returns> true if the string matches the requirements </returns>
    private static bool IsLP(string token)
    {
        string standaloneLPPattern = $"^{LpPattern}$";
        return Regex.IsMatch(token, standaloneLPPattern);
    }

    /// <summary>
    ///   Reports whether "token" is a right parenthesis (closing parenthesis). 
    /// </summary>
    /// <param name="token"> A token that may be an closing parenthesis. </param>
    /// <returns> true if the string matches the requirements </returns>
    private static bool IsRP(string token)
    {
        string standaloneRPPattern = $"^{RpPattern}$";
        return Regex.IsMatch(token, standaloneRPPattern);
    }

    /// <summary>
    ///   <para>
    ///     Given an expression, enumerates the tokens that compose it.
    ///   </para>
    ///   <para>
    ///     Tokens returned are:
    ///   </para>
    ///   <list type="bullet">
    ///     <item>left paren</item>
    ///     <item>right paren</item>
    ///     <item>one of the four operator symbols</item>
    ///     <item>a string consisting of one or more letters followed by one or more numbers</item>
    ///     <item>a double literal</item>
    ///     <item>and anything that doesn't match one of the above patterns</item>
    ///   </list>
    ///   <para>
    ///     There are no empty tokens; white space is ignored (except to separate other tokens).
    ///   </para>
    /// </summary>
    /// <param name="formula"> A string representing an infix formula such as 1*B1/3.0. </param>
    /// <returns> The ordered list of tokens in the formula. </returns>
    private static List<string> GetTokens(string formula)
    {
        List<string> results = [];

        string spacePattern = @"\s+";

        // Overall pattern
        string pattern = string.Format(
                                        "({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                        LpPattern,
                                        RpPattern,
                                        OpPattern,
                                        VariableRegExPattern,
                                        DoublePattern,
                                        spacePattern);

        // Enumerate matching tokens that don't consist solely of white space.
        foreach (string s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
        {
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
            {
                results.Add(s);
            }
        }

        return results;
    }


    // METHODS ADDED FROM PS4

    /// <summary>
    ///   <para>
    ///     Reports whether f1 == f2, using the notion of equality from the <see cref="Equals"/> method.
    ///   </para>
    /// </summary>
    /// <param name="f1"> The first of two formula objects. </param>
    /// <param name="f2"> The second of two formula objects. </param>
    /// <returns> true if the two formulas are the same.</returns>
    public static bool operator ==(Formula f1, Formula f2)
    {
        return f1.ToString().Equals(f2.ToString());
    }

    /// <summary>
    ///   <para>
    ///     Reports whether f1 != f2, using the notion of equality from the <see cref="Equals"/> method.
    ///   </para>
    /// </summary>
    /// <param name="f1"> The first of two formula objects. </param>
    /// <param name="f2"> The second of two formula objects. </param>
    /// <returns> true if the two formulas are not equal to each other.</returns>
    public static bool operator !=(Formula f1, Formula f2)
    {
        return !f1.ToString().Equals(f2.ToString());
    }

    /// <summary>
    ///   <para>
    ///     Determines if two formula objects represent the same formula.
    ///   </para>
    ///   <para>
    ///     By definition, if the parameter is null or does not reference 
    ///     a Formula Object then return false.
    ///   </para>
    ///   <para>
    ///     Two Formulas are considered equal if their canonical string representations
    ///     (as defined by ToString) are equal.  
    ///   </para>
    /// </summary>
    /// <param name="obj"> The other object.</param>
    /// <returns>
    ///   True if the two objects represent the same formula.
    /// </returns>
    public override bool Equals(object? obj)
    {
        if (obj is not Formula)
            return false;

        return this.ToString().Equals(obj.ToString());
    }

    /// <summary>
    ///   <para>
    ///     Evaluates this Formula, using the lookup delegate to determine the values of
    ///     variables.
    ///   </para>
    ///   <remarks>
    ///     When the lookup method is called, it will always be passed a normalized (capitalized)
    ///     variable name.  The lookup method will throw an ArgumentException if there is
    ///     not a definition for that variable token.
    ///   </remarks>
    ///   <para>
    ///     If no undefined variables or divisions by zero are encountered when evaluating
    ///     this Formula, the numeric value of the formula is returned.  Otherwise, a 
    ///     FormulaError is returned (with a meaningful explanation as the Reason property).
    ///   </para>
    ///   <para>
    ///     This method should never throw an exception.
    ///   </para>
    /// </summary>
    /// <param name="lookup">
    ///   <para>
    ///     Given a variable symbol as its parameter, lookup returns the variable's value
    ///     (if it has one) or throws an ArgumentException (otherwise).  This method will expect 
    ///     variable names to be normalized.
    ///   </para>
    /// </param>
    /// <returns> Either a double or a FormulaError, based on evaluating the formula.</returns>
    public object Evaluate(Lookup lookup)
    {
        Stack<double> valStack = new Stack<double>();   //Stack holding values
        Stack<string> opStack = new Stack<string>();    //Stack holding operators

        foreach (string t in tokens)
        {

            // If t is a number
            // If t is a variable
            if (IsNum(t) || IsVar(t))
            {
                // Whether t is a number or variable, parese it to a double / look it up
                double e;
                if(!double.TryParse(t, out e))
                {
                    try
                    {
                        e = lookup(t);
                    }
                    catch (ArgumentException)
                    {
                        return new FormulaError("Cannot lookup variable."); 
                    }
                }

                // Push e onto the value stack
                valStack.Push(e);

                // If the Op Stack has * on top, pop the value stack and multiply that value with t before pushing the result to the val stack.
                if (IsOnTop(opStack, "*"))
                {
                    opStack.Pop();
                    EvalutateMultiplication(valStack);  //Evaulates poppedVal * e
                }
                // If the Op Stack has / on top, pop the value stack and divide that value with t before pushing the result to the val stack.
                else if (IsOnTop(opStack, "/"))
                {
                    opStack.Pop();

                    if (!EvalutateDivision(valStack))
                        return new FormulaError("Tried to divide by zero");
                }
                    
            }

            // If t is + or -
            if (t.Equals("+") || t.Equals("-"))
            {
                // If + or - are on top of the opStack
                if (IsOnTop(opStack, "+"))
                {
                    opStack.Pop();
                    EvalutateAddition(valStack);
                }
                else if (IsOnTop(opStack, "-"))
                {
                    opStack.Pop();
                    EvalutateSubtraction(valStack);
                }

                opStack.Push(t);
            }

            // If t is * or /
            if(t.Equals("*") || t.Equals("/"))
            {
                opStack.Push(t);
            }

            // If t is (
            if (t.Equals("("))
            {
                opStack.Push(t);
            }

            // If t is )
            if (t.Equals(")"))
            {
                // Step 1: If + or - are on top of the opStack, handle them first
                if (IsOnTop(opStack, "+"))
                {
                    opStack.Pop();
                    EvalutateAddition(valStack);
                }
                else if (IsOnTop(opStack, "-"))
                {
                    opStack.Pop();
                    EvalutateSubtraction(valStack);
                }

                // Step 2: Pop the "(" from the operator stack
                opStack.Pop();

                // Step 3: If * or / are on top of the opStack, handle them next
                if (IsOnTop(opStack, "*"))
                {
                    opStack.Pop();
                    EvalutateMultiplication(valStack);
                }
                else if (IsOnTop(opStack, "/"))
                {
                    opStack.Pop();

                    if (!EvalutateDivision(valStack))
                        return new FormulaError("Tried to divide by zero");
                }
            }
        }

        // If + or - are on top of the opStack
        if (IsOnTop(opStack, "+"))
        {
            opStack.Pop();
            EvalutateAddition(valStack);
        }
        else if (IsOnTop(opStack, "-"))
        {
            opStack.Pop();
            EvalutateSubtraction(valStack);
        }

        return valStack.Pop();
    }

    /// <summary>
    /// Checks whether the given string token is on top of the given stack.
    /// </summary>
    /// <param name="stack"></param>
    /// <param name="token"></param>
    /// <returns>Returns true if the token is on top of the stack, false otherwise</returns>
    private bool IsOnTop(Stack<string> stack, string token)
    {
        return stack.TryPeek(out string result) && result.Equals(token);
    }

    /// <summary>
    /// Pulls two values out of the double stack and attempts to calculate multiplication between them before pushing them back into the stack.
    /// </summary>
    /// <param name="stack"></param>
    private void EvalutateMultiplication(Stack<double> stack)
    {
        double b = stack.Pop();
        double a = stack.Pop();

        stack.Push(a * b);
    }

    /// <summary>
    /// Pulls two values out of the double stack and attempts to calculate division between them before pushing them back into the stack.
    /// </summary>
    /// <param name="stack"></param>
    /// <returns>A bool representing whether the evaluation was successful (i.e. dividing by 0 returns false)</returns>
    private bool EvalutateDivision(Stack<double> stack)
    {
        double b = stack.Pop();
        double a = stack.Pop();

        if(b == 0)
            return false; // Trying to divide by 0

        stack.Push(a / b);
        return true;
    }

    /// <summary>
    /// Pulls two values out of the double stack and attempts to calculate addition between them before pushing them back into the stack.
    /// </summary>
    /// <param name="stack"></param>
    private void EvalutateAddition(Stack<double> stack)
    {
        double b = stack.Pop();
        double a = stack.Pop();
        stack.Push(a + b);
    }

    /// <summary>
    /// Pulls two values out of the double stack and attempts to calculate subtraction between them before pushing them back into the stack.
    /// </summary>
    /// <param name="stack"></param>
    private void EvalutateSubtraction(Stack<double> stack)
    {
        double b = stack.Pop();
        double a = stack.Pop();
        stack.Push(a - b);
    }

    //private string 

    /// <summary>
    ///   <para>
    ///     Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    ///     case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two
    ///     randomly-generated unequal Formulas have the same hash code should be extremely small.
    ///   </para>
    /// </summary>
    /// <returns> The hashcode for the object. </returns>
    public override int GetHashCode()
    {
        return this.ToString().GetHashCode();
    }
}


/// <summary>
///   Used to report syntax errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="FormulaFormatException"/> class.
    ///   <para>
    ///      Constructs a FormulaFormatException containing the explanatory message.
    ///   </para>
    /// </summary>
    /// <param name="message"> A developer defined message describing why the exception occured.</param>
    public FormulaFormatException(string message)
        : base(message)
    {
        // All this does is call the base constructor. No extra code needed.
    }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public class FormulaError
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="FormulaError"/> class.
    ///   <para>
    ///     Constructs a FormulaError containing the explanatory reason.
    ///   </para>
    /// </summary>
    /// <param name="message"> Contains a message for why the error occurred.</param>
    public FormulaError(string message)
    {
        Reason = message;
    }

    public string ToString()
    {
        return "ERROR";
    }

    /// <summary>
    ///  Gets the reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}

/// <summary>
///   Any method meeting this type signature can be used for
///   looking up the value of a variable.
/// </summary>
/// <exception cref="ArgumentException">
///   If a variable name is provided that is not recognized by the implementing method,
///   then the method should throw an ArgumentException.
/// </exception>
/// <param name="variableName">
///   The name of the variable (e.g., "A1") to lookup.
/// </param>
/// <returns> The value of the given variable (if one exists). </returns>
public delegate double Lookup(string variableName);
