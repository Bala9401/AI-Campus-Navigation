package com.srec.backend.controller;

import com.srec.backend.entity.Floor;
import com.srec.backend.service.FloorService;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/floors")
public class FloorController {

    private final FloorService floorService;

    public FloorController(FloorService floorService) {
        this.floorService = floorService;
    }

    @GetMapping
    public List<Floor> getAllFloors() {
        return floorService.getAllFloors();
    }
}