using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using CircularRelationshipGraph.Data;
using System.Diagnostics;

namespace CircularRelationshipGraph
{
  public class MinimisedConnectionLengthSort : ISortOrderProvider
  {
    private bool _strengthWeighted;

    public MinimisedConnectionLengthSort(bool strengthWeighted)
    {
      _strengthWeighted = strengthWeighted;
    }

    public INodeList Sort(INodeList nodes)
    {
      // clone the list
      List<INode> sortedList = nodes.ToList();
      for (int i = nodes.Count / 2; i > 1; i--)
      {
        sortedList = MinimizeConnectionLength(sortedList, i);
      }

      return new NodeList(sortedList);
    }

    private List<INode> MinimizeConnectionLength(List<INode> nodes, int pertubation)
    {
      List<INode> optimumConfig = nodes.ToList();

      foreach (var node in nodes)
      {        
        // swap this node with its neighbours
        var testConfiguration = nodes.ToList();
        int index = testConfiguration.IndexOf(node);
        int newIndex = (index + pertubation) % nodes.Count;
        testConfiguration.Swap(index, newIndex);

        // take the best configuration
        optimumConfig = SelectBestConfiguration(testConfiguration, optimumConfig);

        // swap in the other direction
        testConfiguration = nodes.ToList();
        newIndex = Math.Abs(index - pertubation) % nodes.Count;
        testConfiguration.Swap(index, newIndex);

        // take the best configuration
        optimumConfig = SelectBestConfiguration(testConfiguration, optimumConfig);
      }

      return optimumConfig;
    }

    private List<INode> SelectBestConfiguration(List<INode> listOne, List<INode> listTwo)
    {
      double weightOne = ComputeWeight(listOne);
      double weightTwo = ComputeWeight(listTwo);
      return weightOne < weightTwo ? listOne : listTwo;
    }

    /// <summary>
    /// Computes the 'weight' of a given configuration
    /// </summary>
    private double ComputeWeight(List<INode> nodes)
    {
      // create a dictionary from node name to index
      var nodeNameToIndex = nodes.ToDictionary(node => node.Name,
        node => nodes.IndexOf(node));
      
      // iterate over all the nodes
      double weight = 0;
      for (int fromIndex = 0; fromIndex < nodes.Count; fromIndex++)
      {
        INode fromNode = nodes[fromIndex];

        // iterate over this node's relationships
        foreach (var rel in fromNode.Relationships)
        {
          // determine the distance between these nodes
          int toIndex;
          if (nodeNameToIndex.TryGetValue(rel.To, out toIndex))
          {
            int distance = Math.Abs(fromIndex - toIndex);
            if (distance > nodes.Count / 2)
              distance = nodes.Count - distance;

            // add the distance, optionally weighted by connection strength
            weight += _strengthWeighted ? distance * rel.Strength : distance;
          }
        }
      }

      return weight;
    }
  }
}
