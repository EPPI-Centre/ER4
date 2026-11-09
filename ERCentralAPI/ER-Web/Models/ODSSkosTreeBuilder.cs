using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;

public sealed class VocabularyNode
{
    public required string Uri { get; init; }
    public required string Label { get; init; }
    public string? Definition { get; init; }
    public bool IsConceptScheme { get; init; }

    public List<VocabularyNode> Children { get; } = [];
}

public static class SkosTreeBuilder
{
    private static readonly Uri SkosConcept =
        new("http://www.w3.org/2004/02/skos/core#Concept");

    private static readonly Uri SkosBroader =
        new("http://www.w3.org/2004/02/skos/core#broader");

    private static readonly Uri SkosPrefLabel =
        new("http://www.w3.org/2004/02/skos/core#prefLabel");

    private static readonly Uri SkosDefinition =
        new("http://www.w3.org/2004/02/skos/core#definition");

    private static readonly Uri SkosConceptScheme =
        new("http://www.w3.org/2004/02/skos/core#ConceptScheme");

    private static readonly Uri SkosHasTopConcept =
        new("http://www.w3.org/2004/02/skos/core#hasTopConcept");

    private static readonly Uri SkosInScheme =
        new("http://www.w3.org/2004/02/skos/core#inScheme");

    private static readonly Uri DctTitle =
        new("http://purl.org/dc/terms/title");

    public static IReadOnlyList<VocabularyNode> Build(
        string turtle,
        Uri? conceptScheme = null)
    {
        var graph = new Graph();
        var parser = new TurtleParser();
        var rdfType = new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");

        using var reader = new StringReader(turtle);
        parser.Load(graph, reader);

        var concepts = graph
            .GetTriplesWithPredicateObject(
                graph.CreateUriNode(rdfType),
                graph.CreateUriNode(SkosConcept))
            .Select(t => t.Subject)
            .OfType<IUriNode>()
            .Where(node => conceptScheme == null ||
                IsInScheme(graph, node, conceptScheme))
            .ToDictionary(
                node => node.Uri,
                node => new VocabularyNode
                {
                    Uri = node.Uri.AbsoluteUri,
                    Label = GetLiteral(graph, node, SkosPrefLabel)
                        ?? node.Uri.AbsoluteUri,
                    Definition = GetLiteral(graph, node, SkosDefinition)
                });

        var schemes = graph
            .GetTriplesWithPredicateObject(
                graph.CreateUriNode(rdfType),
                graph.CreateUriNode(SkosConceptScheme))
            .Select(t => t.Subject)
            .OfType<IUriNode>()
            .Where(node => conceptScheme == null || node.Uri == conceptScheme)
            .ToDictionary(
                node => node.Uri,
                node => new VocabularyNode
                {
                    Uri = node.Uri.AbsoluteUri,
                    Label = GetLiteral(graph, node, DctTitle)
                        ?? node.Uri.AbsoluteUri,
                    IsConceptScheme = true
                });

        // Select exactly one parent for every concept. A concept's explicit
        // broader relationship takes precedence over its scheme membership.
        var selectedParents = new Dictionary<Uri, Uri>();

        foreach (var conceptUri in concepts.Keys)
        {
            var conceptNode = graph.CreateUriNode(conceptUri);

            var broaderParents = graph
                .GetTriplesWithSubjectPredicate(
                    conceptNode,
                    graph.CreateUriNode(SkosBroader))
                .Select(t => (t.Object as IUriNode)?.Uri)
                .Where(parentUri =>
                    parentUri is not null &&
                    concepts.ContainsKey(parentUri))
                .Select(parentUri => parentUri!)
                .OrderBy(parentUri => parentUri.AbsoluteUri)
                .ToList();

            if (broaderParents.Count > 0)
            {
                selectedParents[conceptUri] = broaderParents[0];
                continue;
            }

            // Top concepts and flat concepts belong directly beneath their
            // concept scheme. hasTopConcept is preferred, with inScheme as
            // a fallback for schemes whose top concepts are not declared.
            var schemeParents = graph
                .GetTriplesWithPredicateObject(
                    graph.CreateUriNode(SkosHasTopConcept),
                    conceptNode)
                .Select(t => (t.Subject as IUriNode)?.Uri)
                .Concat(
                    graph.GetTriplesWithSubjectPredicate(
                            conceptNode,
                            graph.CreateUriNode(SkosInScheme))
                        .Select(t => (t.Object as IUriNode)?.Uri))
                .Where(uri => uri is not null && schemes.ContainsKey(uri))
                .Select(uri => uri!)
                .Distinct()
                .OrderBy(uri => uri.AbsoluteUri)
                .ToList();

            if (schemeParents.Count > 0)
            {
                selectedParents[conceptUri] = schemeParents[0];
            }
        }

        // Attach each child to its selected concept or concept scheme.
        foreach (var relationship in selectedParents)
        {
            var child = concepts[relationship.Key];
            var parent = concepts.TryGetValue(relationship.Value, out var conceptParent)
                ? conceptParent
                : schemes[relationship.Value];

            parent.Children.Add(child);
        }

        // Concept schemes are the roots of the complete vocabulary tree.
        // Concepts without a usable scheme or broader parent remain roots.
        var roots = schemes
            .Where(scheme => !selectedParents.ContainsKey(scheme.Key))
            .Select(scheme => scheme.Value)
            .Concat(concepts
                .Where(concept => !selectedParents.ContainsKey(concept.Key))
                .Select(concept => concept.Value))
            .OrderBy(node => node.Label)
            .ToList();

        return roots;
    }

    private static bool IsInScheme(
        IGraph graph,
        IUriNode concept,
        Uri schemeUri)
    {
        var inScheme = new Uri(
            "http://www.w3.org/2004/02/skos/core#inScheme");

        return graph.ContainsTriple(
            new Triple(
                concept,
                graph.CreateUriNode(inScheme),
                graph.CreateUriNode(schemeUri)));
    }

    private static string? GetLiteral(
        IGraph graph,
        INode subject,
        Uri predicate)
    {
        return graph
            .GetTriplesWithSubjectPredicate(
                subject,
                graph.CreateUriNode(predicate))
            .Select(t => t.Object)
            .OfType<ILiteralNode>()
            .Select(literal => literal.Value)
            .FirstOrDefault();
    }
}