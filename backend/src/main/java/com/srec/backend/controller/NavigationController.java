package com.srec.backend.controller;

import com.srec.backend.entity.Node;
import com.srec.backend.service.PathFindingService;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/navigation")
@CrossOrigin("*")
public class NavigationController {

    private final PathFindingService pathFindingService;

    public NavigationController(PathFindingService pathFindingService) {
        this.pathFindingService = pathFindingService;
    }

    @GetMapping
    public List<Node> findPath(
            @RequestParam int start,
            @RequestParam int end) {

        return pathFindingService.findShortestPath(start, end);
    }
}