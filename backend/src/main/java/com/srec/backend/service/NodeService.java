package com.srec.backend.service;

import com.srec.backend.entity.Node;
import com.srec.backend.repository.NodeRepository;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class NodeService {

    private final NodeRepository nodeRepository;

    public NodeService(NodeRepository nodeRepository) {
        this.nodeRepository = nodeRepository;
    }

    public List<Node> getAllNodes() {
        return nodeRepository.findAll();
    }

}
