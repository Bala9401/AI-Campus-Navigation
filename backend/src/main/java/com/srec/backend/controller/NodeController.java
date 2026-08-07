package com.srec.backend.controller;

import com.srec.backend.entity.Node;
import com.srec.backend.service.NodeService;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/nodes")
@CrossOrigin("*")
public class NodeController {

    private final NodeService nodeService;

    public NodeController(NodeService nodeService) {
        this.nodeService = nodeService;
    }

    @GetMapping
    public List<Node> getAllNodes() {
        return nodeService.getAllNodes();
    }

}