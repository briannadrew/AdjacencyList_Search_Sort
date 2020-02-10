/* Name: Directed Acyclic/Non-Acyclic Graphs, Depth-First Searching & Topological Sorting - Adjacency List Implementation (Assignment #1)
 * Author(s): Brianna Drew & Brian Patrick
 * Date Created: January 18th, 2020
 * Last Modified: February 3rd, 2020
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment1_BriannaDrew_3020_TopologicalSorting
{
    public static class MyGlobals
    {
        public static int clock;
        public static bool exit_bool, error_bool;
    }

    public class Edge<T>
    {
        public Vertex<T> AdjVertex { get; set; }
        public int Cost { get; set; }
        public int Type { get; set; }
        public enum Types { TREE, BACK, FORWARD, CROSS }

        public Edge(Vertex<T> vertex, int cost)
        {
            AdjVertex = vertex;
            Cost = cost;
            Type = 0;
        }
    }

    //---------------------------------------------------------------------------------------------

    public class Vertex<T>
    {
        public T Name { get; set; }              // Vertex name
        public bool Visited { get; set; }
        public int DiscoveryTime { get; set; }
        public int FinishingTime { get; set; }
        public int Colour { get; set; }
        public enum Colours { WHITE, GRAY, BLACK };
        public int InDegree { get; set; }
        public List<Edge<T>> E { get; set; }     // List of adjacency vertices

        public Vertex(T name)
        {
            Name = name;
            Visited = false;
            DiscoveryTime = 0;
            FinishingTime = 0;
            Colour = 0;
            E = new List<Edge<T>>();
        }

        // FindEdge
        // Returns the index of the given adjacent vertex in E; otherwise returns -1
        // Time complexity: O(n) where n is the number of vertices

        public int FindEdge(T name)
        {
            int i;
            for (i = 0; i < E.Count; i++)
            {
                if (E[i].AdjVertex.Name.Equals(name))
                    return i;
            }
            return -1;
        }
    }

    //---------------------------------------------------------------------------------------------

    public interface IDirectedGraph<T>
    {
        void AddVertex(T name);
        void RemoveVertex(T name);
        void AddEdge(T name1, T name2, int cost);
        void RemoveEdge(T name1, T name2);
    }

    //---------------------------------------------------------------------------------------------

    public class DirectedGraph<T> : IDirectedGraph<T>
    {
        private List<Vertex<T>> V;
        private List<Vertex<T>> D;      // for depth-first search sort
        private List<Vertex<T>> S;      // for topological sort
        public bool Acyclic { get; set; }

        public DirectedGraph()
        {
            V = new List<Vertex<T>>();
            D = new List<Vertex<T>>();
            S = new List<Vertex<T>>();
        }

        // FindVertex
        // Returns the index of the given vertex (if found); otherwise returns -1
        // Time complexity: O(n)

        private int FindVertex(T name)
        {
            int i;

            for (i = 0; i < V.Count; i++)
            {
                if (V[i].Name.Equals(name))
                    return i;
            }
            return -1;
        }

        // AddVertex
        // Adds the given vertex to the graph
        // Note: Duplicate vertices are not added
        // Time complexity: O(n) due to FindVertex

        public void AddVertex(T name)
        {
            if (FindVertex(name) == -1)
            {
                Vertex<T> v = new Vertex<T>(name);
                V.Add(v);
                Console.WriteLine("Vertex '" + name + "' created successfully.");
            }
            else
            {
                Console.WriteLine("ERROR: Vertex already exists. Returning to main menu...");
            }
        }

        // RemoveVertex
        // Removes the given vertex and all incident edges from the graph
        // Note:  Nothing is done if the vertex does not exist (except output error statement)
        // Time complexity: O(max(n,m)) where m is the number of edges

        public void RemoveVertex(T name)
        {
            int i, j, k;
            if ((i = FindVertex(name)) > -1)
            {
                for (j = 0; j < V.Count; j++)
                {
                    for (k = 0; k < V[j].E.Count; k++)
                        if (V[j].E[k].AdjVertex.Name.Equals(name))   // Incident edge
                        {
                            V[j].E[k].AdjVertex.InDegree--;
                            V[j].E.RemoveAt(k);
                            break;  // Since there are no duplicate edges
                        }
                }
                V.RemoveAt(i);
                Console.WriteLine("Vertex '" + name + "' removed successfully.");
            }
            else
            {
                Console.WriteLine("ERROR: The selected vertex does not exist. Returning to main menu...");
            }
        }

        // AddEdge
        // Adds the given edge (name1, name2) to the graph
        // Notes: Duplicate edges are not added
        //        By default, the cost of the edge is 0
        // Time complexity: O(n)

        public void AddEdge(T name1, T name2, int cost = 0)
        {
            int i, j;
            Edge<T> e;

            // Do the vertices exist?
            if ((i = FindVertex(name1)) > -1 && (j = FindVertex(name2)) > -1)
            {
                // Does the edge not already exist?
                if (V[i].FindEdge(name2) == -1)
                {
                    e = new Edge<T>(V[j], cost);
                    V[i].E.Add(e);
                    V[j].InDegree++;
                    Console.WriteLine("Edge from vertex '" + name1 + "' to vertex '" + name2 + "' created successfully.");
                }
                else
                {
                    Console.WriteLine("ERROR: Edge already exists. Returning to main menu...");
                }
            }
            else
            {
                Console.WriteLine("ERROR: One or both of the selected vertices do not exist. Returning to main menu...");
            }
        }

        // RemoveEdge
        // Removes the given edge (name1, name2) from the graph
        // Note: Nothing is done if the edge does not exist (except output error statement)
        // Time complexity: O(n)

        public void RemoveEdge(T name1, T name2)
        {
            int i, j;
            if ((i = FindVertex(name1)) > -1 && (j = V[i].FindEdge(name2)) > -1)        // if both edges exist...
            {
                V[j].InDegree--;
                V[i].E.RemoveAt(j);
                Console.WriteLine("Edge from vertex '" + name1 + "' to vertex '" + name2 + "' removed successfully.");
            }
            else
            {
                Console.WriteLine("ERROR: The selected edge does not exist. Returning to main menu...");
            }
        }

        // Depth-First Search
        // Performs a depth-first search (with re-start)
        // Time complexity: O(max,(n,m))

        public void DepthFirstSearch()
        {
            int i, j;
            MyGlobals.clock = 1;

            Console.WriteLine("Depth-first search started...\n");

            for (i = 0; i < V.Count; i++)     // Set all vertices as unvisited
            {
                V[i].Visited = false;
                V[i].DiscoveryTime = 0;
                V[i].FinishingTime = 0;
                V[i].Colour = 0;
            }


            for (i = 0; i < V.Count; i++)
                if (!V[i].Visited)                  // (Re)start with vertex i
                {
                    DepthFirstSearch(V[i]);
                }

            Console.WriteLine("\n");
            Console.WriteLine("Depth-first search completed.");

            for (i = 0; i < V.Count; i++)
                for (j = 0; j < V[i].E.Count; j++)      // Checking ALL edges
                {
                    if (V[i].E[j].Type == 1)        // If the current edge is a BACK edge
                    {
                        Acyclic = false;
                        goto Acyclicity;
                    }
                    else
                    {
                        Acyclic = true;
                    }
                }

            Acyclicity:
            if (Acyclic)
            {
                Console.WriteLine("This graph is acyclic.\n");
                DepthFirstSearchSort();
                TopologicalSort();
            }
            else
            {
                Console.WriteLine("This graph is not acyclic.");
            }
        }

        private void DepthFirstSearch(Vertex<T> v)
        {
            int j;
            Vertex<T> w;

            v.Visited = true;       // Mark vertex as visited
            v.Colour = 1;
            v.DiscoveryTime = MyGlobals.clock;
            MyGlobals.clock++;
            Console.WriteLine("Visiting vertex '" + v.Name + "' for the first time at a time of " + v.DiscoveryTime + " and is now " + Enum.GetName(typeof(Vertex<T>.Colours), v.Colour) + ".");      // Output vertex when marked as visited
            for (j = 0; j < v.E.Count; j++)       // Visit next adjacent vertex
            {
                w = v.E[j].AdjVertex;       // Find index of adjacent vertex in V
                if (!w.Visited)
                {
                    v.E[j].Type = 0;        // the edge is a TREE edge
                    DepthFirstSearch(w);
                }
                else
                {
                    if (w.Colour == 1)       // if the adjacent vertex is GRAY
                    {
                        v.E[j].Type = 1;        // the edge is a BACK edge
                    }
                    else
                    {
                        if (w.DiscoveryTime > v.DiscoveryTime)       // if the adjacent vertex's discovery time is greater than the current vertex's discovery time
                        {
                            v.E[j].Type = 2;        // the edge is a FORWARD edge
                        }
                        else        // if the adjacent vertex's discovery time is less than the current vertex's discovery time
                        {
                            v.E[j].Type = 3;        // the edge is a CROSS edge
                        }
                    }
                }
                Console.WriteLine("Edge from vertex '" + v.Name + "' to vertex '" + w.Name + "' is a " + Enum.GetName(typeof(Edge<T>.Types), v.E[j].Type) + " edge."); // print out names of vertices and type of edge
            }
            v.Colour = 2;       // Mark vertex as finished
            v.FinishingTime = MyGlobals.clock;
            MyGlobals.clock++;
            Console.WriteLine("Vertex '" + v.Name + "' finished at a time of " + v.FinishingTime + " and is now " + Enum.GetName(typeof(Vertex<T>.Colours), v.Colour) + ".");
        }

        // Depth-First Search Sort
        // Performs a depth-first search (with re-start) and adds vertex to end of a list once BLACK (finished)
        // Time complexity: O(max,(n,m))
        public void DepthFirstSearchSort()
        {
            int i;
            MyGlobals.clock = 1;

            for (i = 0; i < V.Count; i++)     // Set all vertices as unvisited
            {
                V[i].Visited = false;
                V[i].DiscoveryTime = 0;
                V[i].FinishingTime = 0;
                V[i].Colour = 0;
            }

            for (i = 0; i < V.Count; i++)
                if (!V[i].Visited)                  // (Re)start with vertex i
                {
                    DepthFirstSearchSort(V[i]);
                }

            Console.WriteLine("Vertices Sorted Via Depth-First Search:");
            for (i = 0; i < D.Count; i++)
                Console.WriteLine(D[i].Name);
            Console.WriteLine("\n");
        }

        public void DepthFirstSearchSort(Vertex<T> v)
        {
            int j;
            Vertex<T> w;

            v.Visited = true;       // Mark vertex as visited
            v.Colour = 1;
            v.DiscoveryTime = MyGlobals.clock;
            MyGlobals.clock++;

            for (j = 0; j < v.E.Count; j++)       // Visit next adjacent vertex
            {
                w = v.E[j].AdjVertex;       // Find index of adjacent vertex in V
                if (!w.Visited)
                {
                    v.E[j].Type = 0;        // the edge is a TREE edge
                    DepthFirstSearchSort(w);
                }
                else
                {
                    if (w.Colour == 1)       // if the adjacent vertex is GRAY
                    {
                        v.E[j].Type = 1;        // the edge is a BACK edge
                    }
                    else
                    {
                        if (w.DiscoveryTime > v.DiscoveryTime)       // if the adjacent vertex's discovery time is greater than the current vertex's discovery time
                        {
                            v.E[j].Type = 2;        // the edge is a FORWARD edge
                        }
                        else        // if the adjacent vertex's discovery time is less than the current vertex's discovery time
                        {
                            v.E[j].Type = 3;        // the edge is a CROSS edge
                        }
                    }
                }
            }
            v.Colour = 2;       // Mark vertex as finished
            v.FinishingTime = MyGlobals.clock;
            MyGlobals.clock++;

            if (D.Count > 0)        // If there are exisiting vertices in new list D...
            {
                D.Insert(0, v);     // Insert vertice at front of the list
            }
            else        // If there are existing vertices in the list...
            {
                D.Add(v);       // Add vertice to list
            }
        }

        // Topological Sort
        // Repeatedly finds an un-visted vertice with an in-degree of 0, marks it as visited, and adds it to the end of a list. All the in-degrees of the remaining adjacent vertices are decreased by 1 each time.
        public void TopologicalSort()
        {
            int i, j, remaining_vertices = V.Count;
            MyGlobals.clock = 1;
            int[] In_Degrees = new int[V.Count];      // Create new array to store the temporary in-degrees of all vertices (as we do not want to make changes to the actual graph)

            for (i = 0; i < V.Count; i++)     // Set all vertices as unvisited
            {
                V[i].Visited = false;
                V[i].DiscoveryTime = 0;
                V[i].FinishingTime = 0;
                V[i].Colour = 0;
                In_Degrees[i] = V[i].InDegree;      // Add all in-degrees of each vertex to the corresponding index in the In_Degrees array
            }

            while (remaining_vertices > 0)      // Keep looping through vertices until all have been added to the list and have been "visited" (all have a temporary in-degree of 0)
            {
                for (i = 0; i < V.Count; i++)
                {
                    if (!V[i].Visited && In_Degrees[i] == 0)        // If a vertex is unvisited and has an in degree of 0...
                    {
                        V[i].Visited = true;        // Mark the vertex as visited
                        V[i].Colour = 1;
                        V[i].DiscoveryTime = MyGlobals.clock;
                        MyGlobals.clock++;
                        S.Add(V[i]);        // Add vertex to the end of list S
                        remaining_vertices--;
                    }
                    for (j = 0; j < V.Count; j++)
                    {
                        if (V[i].FindEdge(V[j].Name) > -1 && In_Degrees[j] > 0)      // Check all remaining vertices v to see if there is an edge that exists from u (V[i]) to v (V[j]) and if so...
                        {
                            In_Degrees[j]--;        // Decrease the temporary in degree by 1
                        }
                    }
                }
            }
            Console.WriteLine("Topologically Sorted Vertices:");
            for (i = 0; i < S.Count; i++)
                Console.WriteLine(S[i].Name);
        }

        // Print Graph
        // prints contents of graph (vertices nd edges)
        public void PrintGraph()
        {
            int i, j;
            if (V.Count != 0)
            {
                Console.WriteLine("Vertices:");
                for (i = 0; i < V.Count; i++)
                    Console.WriteLine("Name: " + V[i].Name + "; In-Degree: " + V[i].InDegree);
                Console.WriteLine("\n");
                Console.WriteLine("Edges:");
                for (i = 0; i < V.Count; i++)
                    for (j = 0; j < V[i].E.Count; j++)
                        Console.WriteLine("(" + V[i].Name + " --> " + V[i].E[j].AdjVertex.Name + ", " + V[i].E[j].Cost + ")");
            }
            else
            {
                Console.WriteLine("Graph has no edges and no vertices.");
            }
        }

        // Empty Graph
        // clears all vertices and edges from graph
        public void EmptyGraph()
        {
            int i, j;
            for (i = 0; i < V.Count; i++)
                for (j = 0; j < V[i].E.Count; j++)
                    V[i].E.Clear();     // clear every adjacency list of edges for each vertex
            V.Clear();      // clear list of vertices
        }
    }

    // Test Class

    class Program
    {
        static void Main(string[] args)
        {
            int intchoice, int_weight;
            string choice, vchoice, echoice1, echoice2, weight;
            bool conversion;
            MyGlobals.exit_bool = false;
            DirectedGraph<string> G = new DirectedGraph<string>();


            while (!MyGlobals.exit_bool)
            {
                MyGlobals.error_bool = true;
                Console.WriteLine("******************************************************");
                Console.WriteLine("*                                                    *");
                Console.WriteLine("*               DIRECTED ACYCLIC GRAPHS              *");
                Console.WriteLine("*            ADJACENCY LIST IMPLEMENTATION           *");
                Console.WriteLine("*                                                    *");
                Console.WriteLine("******************************************************");
                Console.WriteLine("*                                                    *");
                Console.WriteLine("*    1 = CREATE A NEW GRAPH                          *");
                Console.WriteLine("*    2 = ADD A NEW VERTEX                            *");
                Console.WriteLine("*    3 = ADD A NEW EDGE                              *");
                Console.WriteLine("*    4 = REMOVE A VERTEX                             *");
                Console.WriteLine("*    5 = REMOVE AN EDGE                              *");
                Console.WriteLine("*    6 = PERFORM A DEPTH-FIRST SEARCH                *");
                Console.WriteLine("*    7 = PRINT A GRAPH                               *");
                Console.WriteLine("*    8 = EXIT PROGRAM                                *");
                Console.WriteLine("*                                                    *");
                Console.WriteLine("******************************************************\n");

                do
                {
                    choice = Console.ReadLine();        // get user choice
                    conversion = Int32.TryParse(choice, out intchoice);
                    if (!conversion)        // if the user's entry is not an integer
                    {
                        Console.WriteLine("ERROR: Invalid option. Please try again.\n");
                    }
                    else
                    {
                        MyGlobals.error_bool = false;
                        break;
                    }
                    Console.WriteLine();
                } while (MyGlobals.error_bool);

                MyGlobals.error_bool = true;
                Console.WriteLine();

                switch (intchoice)
                {
                    case 1:     // Create new graph
                        G.EmptyGraph();
                        Console.WriteLine("New graph created successfully.\n");
                        break;
                    case 2:     // Add a new vertex
                        Console.WriteLine("What would you like to name the new vertex?");
                        vchoice = Console.ReadLine();
                        G.AddVertex(vchoice);
                        Console.WriteLine("\n");
                        break;
                    case 3:     // Add a new edge
                        Console.WriteLine("What vertex would you like the edge to BEGIN at?");
                        echoice1 = Console.ReadLine();
                        Console.WriteLine("What vertex would you like the edge to END at?");
                        echoice2 = Console.ReadLine();
                        Console.WriteLine("What would you like the WEIGHT of the edge to be?");
                        do
                        {
                            weight = Console.ReadLine();
                            conversion = Int32.TryParse(weight, out int_weight);
                            if (!conversion)        // if the user's entry is not an integer
                            {
                                Console.WriteLine("ERROR: Invalid weight. Please try again.\n");
                            }
                            else
                            {
                                MyGlobals.error_bool = false;
                                break;
                            }
                        } while (MyGlobals.error_bool);

                        G.AddEdge(echoice1, echoice2, int_weight);
                        Console.WriteLine("\n");
                        break;
                    case 4:     // Remove a vertex
                        Console.WriteLine("Which vertex would you like to remove?");
                        vchoice = Console.ReadLine();
                        G.RemoveVertex(vchoice);
                        Console.WriteLine("\n");
                        break;
                    case 5:     // Remove an edge
                        Console.WriteLine("What is the vertex that the edge you would like to remove BEGINS at?");
                        echoice1 = Console.ReadLine();
                        Console.WriteLine("What is the vertex that the edge you would like to remove ENDS at?");
                        echoice2 = Console.ReadLine();
                        G.RemoveEdge(echoice1, echoice2);
                        Console.WriteLine("\n");
                        break;
                    case 6:     // Perform a depth-first search
                        G.DepthFirstSearch();
                        Console.WriteLine("\n");
                        break;
                    case 7:     // Print graph contents
                        G.PrintGraph();
                        Console.WriteLine("\n");
                        break;
                    case 8:     // Exit program
                        MyGlobals.exit_bool = true;
                        Console.WriteLine("Exiting program...");
                        Environment.Exit(0);
                        break;
                    default:        // Invalid integer menu choice
                        Console.WriteLine("ERROR: Invalid option. Please try again.\n");
                        break;
                }
            }
        }
    }
}