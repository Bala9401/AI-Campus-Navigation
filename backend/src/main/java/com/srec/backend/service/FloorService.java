package com.srec.backend.service;

import com.srec.backend.entity.Floor;
import com.srec.backend.repository.FloorRepository;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class FloorService {

    private final FloorRepository floorRepository;

    public FloorService(FloorRepository floorRepository) {
        this.floorRepository = floorRepository;
    }

    public List<Floor> getAllFloors() {
        return floorRepository.findAll();
    }
}