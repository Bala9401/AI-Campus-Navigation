package com.srec.backend.controller;

import com.srec.backend.entity.Edge;
import com.srec.backend.service.EdgeService;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/edges")
@CrossOrigin("*")
public class EdgeController {

    private final EdgeService edgeService;

    public EdgeController(EdgeService edgeService) {
        this.edgeService = edgeService;
    }

    @GetMapping
    public List<Edge> getAllEdges() {
        return edgeService.getAllEdges();
    }
}