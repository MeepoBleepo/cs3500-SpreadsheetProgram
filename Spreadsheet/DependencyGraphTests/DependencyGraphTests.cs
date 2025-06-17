// <authors> Logan Wood </authors>
// <date> September 13, 2024 </date>

namespace CS3500.DevelopmentTests;

using CS3500.DependencyGraph;

/// <summary>
///   This is a test class for DependencyGraphTest and is intended
///   to contain all DependencyGraphTest Unit Tests
/// </summary>
[TestClass]
public class DependencyGraphTests
{
    /// <summary>
    ///   This test is stressing the DependencyGraph class by performing a high volume of operations in sequence, including:
    ///   <list type="bullet">
    ///     <item>
    ///         Adding a bunch of dependencies
    ///     </item>
    ///     <item>
    ///         Removing a bunch of dependencies
    ///     </item>
    ///     <item>
    ///         Re-adding a some dependencies
    ///     </item>
    ///     <item>
    ///         Removing some more dependencies
    ///     </item>
    ///   </list>
    ///   The test checks that the DependencyGraph handles all of these operations efficiently and correctly.
    /// </summary>
    [TestMethod]
    [Timeout(2000)]  // 2 second run time limit
    public void StressTest()
    {
        DependencyGraph dg = new();

        // A bunch of strings to use
        const int SIZE = 200;
        string[] letters = new string[SIZE];
        for (int i = 0; i < SIZE; i++)
        {
            letters[i] = string.Empty + ((char)('a' + i));
        }

        // The correct answers
        HashSet<string>[] dependents = new HashSet<string>[SIZE];
        HashSet<string>[] dependees = new HashSet<string>[SIZE];
        for (int i = 0; i < SIZE; i++)
        {
            dependents[i] = [];
            dependees[i] = [];
        }

        // Add a bunch of dependencies
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = i + 1; j < SIZE; j++)
            {
                dg.AddDependency(letters[i], letters[j]);
                dependents[i].Add(letters[j]);
                dependees[j].Add(letters[i]);
            }
        }

        // Remove a bunch of dependencies
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = i + 4; j < SIZE; j += 4)
            {
                dg.RemoveDependency(letters[i], letters[j]);
                dependents[i].Remove(letters[j]);
                dependees[j].Remove(letters[i]);
            }
        }

        // Add some back
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = i + 1; j < SIZE; j += 2)
            {
                dg.AddDependency(letters[i], letters[j]);
                dependents[i].Add(letters[j]);
                dependees[j].Add(letters[i]);
            }
        }

        // Remove some more
        for (int i = 0; i < SIZE; i += 2)
        {
            for (int j = i + 3; j < SIZE; j += 3)
            {
                dg.RemoveDependency(letters[i], letters[j]);
                dependents[i].Remove(letters[j]);
                dependees[j].Remove(letters[i]);
            }
        }

        // Make sure everything is right
        for (int i = 0; i < SIZE; i++)
        {
            Assert.IsTrue(dependents[i].SetEquals(new HashSet<string>(dg.GetDependents(letters[i]))));
            Assert.IsTrue(dependees[i].SetEquals(new HashSet<string>(dg.GetDependees(letters[i]))));
        }
    }

    //Test Size

    [TestMethod]
    public void TestSize_TwoPairs_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        dg.AddDependency("A1", "B2");
        dg.AddDependency("A1", "D1");

        Assert.AreEqual(2, dg.Size);
    }

    [TestMethod]
    public void TestSize_Zero_Valid()
    {
        DependencyGraph dg = new DependencyGraph();

        Assert.AreEqual(0, dg.Size);
    }

    [TestMethod]
    public void TestSize_AfterReplace_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        dg.AddDependency("A1", "B2");   // Size = 1
        Assert.AreEqual(1, dg.Size);

        List<string> newDependents = new List<string>();
        newDependents.Add("D4");
        newDependents.Add("E5");

        dg.ReplaceDependents("A1", newDependents);  // Size = 2
        Assert.AreEqual(2, dg.Size);

    }

    //Test HasDependents

    /// <summary>
    /// Tests that HasDependents returns true when a dependee does have dependents.
    /// </summary>
    [TestMethod]
    public void TestHasDependents_TwoDependents_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        dg.AddDependency("A1", "B2");
        dg.AddDependency("A1", "D1");
        Assert.IsTrue(dg.HasDependents("A1"));
    }

    /// <summary>
    /// Tests that HasDependents returns false when the input dependee doesn't exist.
    /// </summary>
    [TestMethod]
    public void TestHasDependents_NoDependents_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        Assert.IsFalse(dg.HasDependents("A1"));
    }

    //Test HasDependees

    /// <summary>
    /// Tests that HasDependees returns true when a dependent does have dependees.
    /// </summary>
    [TestMethod]
    public void TestHasDependees_TwoDependees_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        dg.AddDependency("A1", "C3");
        dg.AddDependency("B2", "C3");
        Assert.IsTrue(dg.HasDependees("C3"));
    }

    /// <summary>
    /// Tests that HasDependees returns False when the input dependent doesn't exist.
    /// </summary>
    [TestMethod]
    public void TestHasDependees_NoDependees_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        Assert.IsFalse(dg.HasDependees("A1"));
    }

    //Test GetDependents

    /// <summary>
    /// Tests that GetDependents returns a list containing the multiple 
    /// dependents of a cell.
    /// </summary>
    [TestMethod]
    public void TestGetDependents_TwoEntries_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        dg.AddDependency("A1", "B2");
        dg.AddDependency("A1", "D1");
        HashSet<string> dependents = (HashSet<string>)dg.GetDependents("A1");
        Assert.IsTrue(dependents.TryGetValue("B2", out string? a));
        Assert.IsTrue(dependents.TryGetValue("D1", out string? b));
        Assert.IsFalse(dependents.TryGetValue("A1", out string? c));
    }

    //Test GetDependees

    /// <summary>
    /// Tests that GetDependees returns a list containing the multiple 
    /// dependees of a cell.
    /// </summary>
    [TestMethod]
    public void TestGetDependees_TwoEntries_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        dg.AddDependency("A1", "D1");
        dg.AddDependency("B2", "D1");
        HashSet<string> dependees = (HashSet<string>)dg.GetDependees("D1");
        Assert.IsTrue(dependees.TryGetValue("A1", out string? a));
        Assert.IsTrue(dependees.TryGetValue("B2", out string? b));
        Assert.IsFalse(dependees.TryGetValue("D1", out string? c));
    }

    //Test AddDependency

    /// <summary>
    /// Tests that AddDependency does not throw any errors when adding
    /// a new dependee dependent pair.
    /// </summary>
    [TestMethod]
    public void TestAddDependency_AddOne_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        dg.AddDependency("A1", "B2");
    }

    /// <summary>
    /// Tests that AddDependency does not throw any errors when adding another
    /// dependent to an already existing dependee.
    /// </summary>
    [TestMethod]
    public void TestAddDependency_AddTwoDependee_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        dg.AddDependency("A1", "B2");
        dg.AddDependency("A1", "C3");
    }

    //Test RemoveDependency

    /// <summary>
    /// Tests RemoveDependency, checking that the removed dependee's dependents are
    /// updated accordingly.
    /// </summary>
    [TestMethod]
    public void TestRemoveDependency_CheckDependeePostRemove_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        dg.AddDependency("A1", "B2");
        dg.AddDependency("A1", "C3");

        dg.RemoveDependency("A1", "B2"); //Method being tested

        HashSet<string> dependents = (HashSet<string>)dg.GetDependents("A1");
        Assert.IsTrue(dependents.TryGetValue("C3", out string? a));
        Assert.IsFalse(dependents.TryGetValue("B2", out string? b));
    }

    /// <summary>
    /// Tests RemoveDependency, checking that the removed dependent's dependees are
    /// updated accordingly.
    /// </summary>
    [TestMethod]
    public void TestRemoveDependency_CheckDependentPostRemove_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        dg.AddDependency("A1", "B2");
        dg.AddDependency("A1", "C3");
        dg.AddDependency("D4", "C3");

        dg.RemoveDependency("A1", "C3"); //Method being tested

        HashSet<string> dependents = (HashSet<string>)dg.GetDependees("C3");
        Assert.IsTrue(dependents.TryGetValue("D4", out string? a));
        Assert.IsFalse(dependents.TryGetValue("A1", out string? b));
    }

    /// <summary>
    /// Tests RemoveDependency for a pair which do not exist in the DG
    /// </summary>
    [TestMethod]
    public void TestRemoveDependency_DoesntExist_Valid()
    {
        DependencyGraph dg = new DependencyGraph();

        dg.RemoveDependency("A1", "C3"); //Method being tested
    }

    //ReplaceDependents

    /// <summary>
    /// Tests that ReplaceDependents will replace the dependents of a dependee and remove the old dependents.
    /// </summary>
    [TestMethod]
    public void TestReplaceDependents_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        dg.AddDependency("A1", "B2");

        List<string> newDependents = new List<string>();
        newDependents.Add("D4");
        newDependents.Add("E5");

        dg.ReplaceDependents("A1", newDependents);

        HashSet<string> dependents = (HashSet<string>)dg.GetDependents("A1");
        Assert.IsTrue(dependents.TryGetValue("D4", out string? a));
        Assert.IsTrue(dependents.TryGetValue("E5", out string? b));
        Assert.IsFalse(dependents.TryGetValue("B2", out string? c));
    }

    //ReplaceDependees

    /// <summary>
    /// Tests that ReplaceDependees will replace the dependees of a dependent and remove the old dependees.
    /// </summary>
    [TestMethod]
    public void TestReplaceDependees_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        dg.AddDependency("A1", "B2");

        List<string> newDependees = new List<string>();
        newDependees.Add("D4");
        newDependees.Add("E5");

        dg.ReplaceDependees("B2", newDependees);

        HashSet<string> dependees = (HashSet<string>)dg.GetDependees("B2");
        Assert.IsTrue(dependees.TryGetValue("D4", out string? a));
        Assert.IsTrue(dependees.TryGetValue("E5", out string? b));
        Assert.IsFalse(dependees.TryGetValue("A1", out string? c));
    }

}