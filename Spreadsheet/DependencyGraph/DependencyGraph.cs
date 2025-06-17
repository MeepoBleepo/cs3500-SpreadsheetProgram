// Skeleton implementation written by Joe Zachary for CS 3500, September 2013
// Version 1.1 - Joe Zachary
//   (Fixed error in comment for RemoveDependency)
// Version 1.2 - Daniel Kopta Fall 2018
//   (Clarified meaning of dependent and dependee)
//   (Clarified names in solution/project structure)
// Version 1.3 - H. James de St. Germain Fall 2024

// Complete implementation written by Logan Wood for CS 3500, September 13, 2024

namespace CS3500.DependencyGraph;

/// <summary>
///   <para>
///     (s1,t1) is an ordered pair of strings, meaning t1 depends on s1.
///     (in other words: s1 must be evaluated before t1.)
///   </para>
///   <para>
///     A DependencyGraph can be modeled as a set of ordered pairs of strings.
///     Two ordered pairs (s1,t1) and (s2,t2) are considered equal if and only
///     if s1 equals s2 and t1 equals t2.
///   </para>
///   <remarks>
///     Recall that sets never contain duplicates.
///     If an attempt is made to add an element to a set, and the element is already
///     in the set, the set remains unchanged.
///   </remarks>
///   <para>
///     Given a DependencyGraph DG:
///   </para>
///   <list type="number">
///     <item>
///       If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
///       (The set of things that depend on s.)
///     </item>
///     <item>
///       If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
///       (The set of things that s depends on.)
///     </item>
///   </list>
///   <para>
///      For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}.
///   </para>
///   <code>
///     dependents("a") = {"b", "c"}
///     dependents("b") = {"d"}
///     dependents("c") = {}
///     dependents("d") = {"d"}
///     dependees("a")  = {}
///     dependees("b")  = {"a"}
///     dependees("c")  = {"a"}
///     dependees("d")  = {"b", "d"}
///   </code>
/// </summary>
public class DependencyGraph
{
    private Dictionary<String, HashSet<String>> dependents = new Dictionary<String, HashSet<String>>();
    private Dictionary<String, HashSet<String>> dependees = new Dictionary<String, HashSet<String>>();
    private int size = 0;

    /// <summary>
    ///   Initializes a new instance of the <see cref="DependencyGraph"/> class.
    ///   The initial DependencyGraph is empty.
    /// </summary>
    public DependencyGraph()
    {
        dependents = new Dictionary<String, HashSet<String>>();
        dependees = new Dictionary<String, HashSet<String>>();
    }

    /// <summary>
    /// The number of ordered pairs in the DependencyGraph.
    /// </summary>
    public int Size
    {
        get { return size; }
    }

    /// <summary>
    ///   Reports whether the given node has dependents (i.e., other nodes depend on it).
    /// </summary>
    /// <param name="nodeName"> The name of the node.</param>
    /// <returns> true if the node has dependents. </returns>
    public bool HasDependents(string nodeName)
    {
        HashSet<String>? values; //The HashSet of dependent nodes
        if(dependents.TryGetValue(nodeName, out values) ) 
            return values.Count > 0;
        return false;
    }

    /// <summary>
    ///   Reports whether the given node has dependees (i.e., depends on one or more other nodes).
    /// </summary>
    /// <returns> true if the node has dependees.</returns>
    /// <param name="nodeName">The name of the node.</param>
    public bool HasDependees(string nodeName)
    {
        HashSet<String>? values; //The HashSet of dependee nodes
        if (dependees.TryGetValue(nodeName, out values))
            return values.Count > 0;
        return false;
    }

    /// <summary>
    ///   <para>
    ///     Returns the dependents of the node with the given name.
    ///   </para>
    /// </summary>
    /// <param name="nodeName"> The node we are looking at.</param>
    /// <returns> The dependents of nodeName. </returns>
    public IEnumerable<string> GetDependents(string nodeName)
    {
        HashSet<String>? values; //The HashSet of dependee nodes
        if (dependents.TryGetValue(nodeName, out values))
            return values;
        return new HashSet<String>(); //Return empty dependent set if this node does not have one
    }

    /// <summary>
    ///   <para>
    ///     Returns the dependees of the node with the given name.
    ///   </para>
    /// </summary>
    /// <param name="nodeName"> The node we are looking at.</param>
    /// <returns> The dependees of nodeName. </returns>
    public IEnumerable<string> GetDependees(string nodeName)
    {
        HashSet<String>? values; //The HashSet of dependee nodes
        if (dependees.TryGetValue(nodeName, out values))
            return values;
        return new HashSet<String>(); //Return empty dependent set if this node does not have one
    }

    /// <summary>
    /// <para>Adds the ordered pair (dependee, dependent), if it doesn't exist.</para>
    ///
    /// <para>
    ///   This can be thought of as: dependee must be evaluated before dependent
    /// </para>
    /// </summary>
    /// <param name="dependee"> the name of the node that must be evaluated first</param>
    /// <param name="dependent"> the name of the node that cannot be evaluated until after dependee</param>
    public void AddDependency(string dependee, string dependent)
    {

        //UPDATE DEPENDENTS MAP
        HashSet<String>? dependentValues;
        //Get the HashSet of dependent nodes if it exists
        if (dependents.TryGetValue(dependee, out dependentValues))
        {
            //Remove the dependent (if it doesn't already exist)
            if(dependentValues.Add(dependent))
                //Update size
                size++;
        }
        //If the key does not already exist, add it to the map
        else
        {
            HashSet<string> values = new HashSet<string>(); //The new set of dependents 
            values.Add(dependent); //Add the dependent
            dependents.Add(dependee, values);
            
            //Update size
            size++;
        }

        //UPDATE DEPENDEES MAP
        HashSet<String>? dependeeValues;
        //Get the HashSet of dependee nodes if it exists
        if (dependees.TryGetValue(dependent, out dependeeValues))
        {
            //Add the dependee (if it doesn't already exist)
            dependeeValues.Add(dependee);
        }
        //If the key does not already exist, add it to the map
        else
        {
            HashSet<string> values = new HashSet<string>(); //The new set of dependents 
            values.Add(dependee); //Add the dependent
            dependees.Add(dependent, values);
        }
    }

    /// <summary>
    ///   <para>
    ///     Removes the ordered pair (dependee, dependent), if it exists.
    ///   </para>
    /// </summary>
    /// <param name="dependee"> The name of the node that must be evaluated first</param>
    /// <param name="dependent"> The name of the node that cannot be evaluated until after dependee</param>
    public void RemoveDependency(string dependee, string dependent)
    {
        //UPDATE DEPENDENTS MAP
        HashSet<String>? dependentValues;
        //Get the HashSet of dependent nodes if it exists
        if (dependents.TryGetValue(dependee, out dependentValues))
        {
            //Remove the dependent (if it exists)
            if(dependentValues.Remove(dependent))
                //Update size
                size--;
        }

        //UPDATE DEPENDEES MAP
        HashSet<String>? dependeeValues;
        //Get the HashSet of dependee nodes if it exists
        if (dependees.TryGetValue(dependent, out dependeeValues))
        {
            //Remove the dependee (if it exists)
            dependeeValues.Remove(dependee);
        }

        
    }

    /// <summary>
    ///   Removes all existing ordered pairs of the form (nodeName, *).  Then, for each
    ///   t in newDependents, adds the ordered pair (nodeName, t).
    /// </summary>
    /// <param name="nodeName"> The name of the node who's dependents are being replaced </param>
    /// <param name="newDependents"> The new dependents for nodeName</param>
    public void ReplaceDependents(string nodeName, IEnumerable<string> newDependents)
    {
        IEnumerable<string>? oldDependents = GetDependents(nodeName);
        
        //Remove all existing ordered pairs
        foreach (string s in oldDependents)
        {
            RemoveDependency(nodeName, s);
        }

        //Add new ordered pairs
        foreach (string s in newDependents)
        {
            AddDependency(nodeName, s);
        }
    }

    /// <summary>
    ///   <para>
    ///     Removes all existing ordered pairs of the form (*, nodeName).  Then, for each
    ///     t in newDependees, adds the ordered pair (t, nodeName).
    ///   </para>
    /// </summary>
    /// <param name="nodeName"> The name of the node who's dependees are being replaced</param>
    /// <param name="newDependees"> The new dependees for nodeName</param>
    public void ReplaceDependees(string nodeName, IEnumerable<string> newDependees)
    {
        IEnumerable<string>? oldDependees = GetDependees(nodeName);

        //Remove all existing ordered pairs
        foreach (string s in oldDependees)
        {
            RemoveDependency(s, nodeName);
        }

        //Add new ordered pairs
        foreach (string s in newDependees)
        {
            AddDependency(s, nodeName);
        }
    }
}