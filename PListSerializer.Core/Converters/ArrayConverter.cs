﻿using PListNet;
using PListNet.Nodes;

namespace PListSerializer.Core.Converters;

internal sealed class ArrayConverter<TElement>(IPlistConverter<TElement> elementConverter) : IPlistConverter<TElement[]>
{
    public TElement[] Deserialize(PNode rootNode)
        => rootNode is ArrayNode arrayNode
            ? arrayNode.Select(elementConverter.Deserialize).ToArray()
            : default;
}
