package com.srec.backend.service;
import com.srec.backend.entity.Node;
import com.srec.backend.repository.NodeRepository;
import com.srec.backend.entity.Edge;
import com.srec.backend.repository.EdgeRepository;
import org.springframework.stereotype.Service;
import java.util.Map;
import java.util.HashMap;
import java.util.ArrayList;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.PriorityQueue;

@Service
public class PathFindingService {

    private final EdgeRepository edgeRepository;
    private final NodeRepository nodeRepository;

   public PathFindingService(EdgeRepository edgeRepository,
                          NodeRepository nodeRepository) {

    this.edgeRepository = edgeRepository;
    this.nodeRepository = nodeRepository;
}

    public List<Edge> getAllEdges() {
        return edgeRepository.findAll();
    }
    private Map<Integer, List<Edge>> buildGraph() {

    Map<Integer, List<Edge>> graph = new HashMap<>();

    for (Edge edge : edgeRepository.findAll()) {

        graph.putIfAbsent(edge.getFromNode(), new ArrayList<>());

        graph.get(edge.getFromNode()).add(edge);

    }

    return graph;
}
public List<Node> findShortestPath(int startNode, int endNode) {

    Map<Integer, List<Edge>> graph = buildGraph();
    Map<Integer, Double> distances = new HashMap<>();
    Map<Integer, Integer> previous = new HashMap<>();
    PriorityQueue<Integer> queue =
        new PriorityQueue<>((a, b) ->
                Double.compare(
                        distances.getOrDefault(a, Double.MAX_VALUE),
                        distances.getOrDefault(b, Double.MAX_VALUE)
                )
        );
        distances.put(startNode, 0.0);
         queue.add(startNode);
          while (!queue.isEmpty()) {

        int currentNode = queue.poll();

        if (currentNode == endNode) {
            break;
        }
        List<Edge> neighbors = graph.getOrDefault(currentNode, new ArrayList<>());
        for (Edge edge : neighbors) {
            int nextNode = edge.getToNode();
            double newDistance =
        distances.get(currentNode) + edge.getDistance();
        if (newDistance < distances.getOrDefault(nextNode, Double.MAX_VALUE)) {

    distances.put(nextNode, newDistance);

    previous.put(nextNode, currentNode);

    queue.add(nextNode);
}

}

    }
    List<Integer> path = new ArrayList<>();

Integer current = endNode;

while (current != null) {

    path.add(0, current);

    current = previous.get(current);
}

List<Node> result = new ArrayList<>();

for (Integer nodeId : path) {

    nodeRepository.findById(nodeId).ifPresent(result::add);

}

return result;
    }
}