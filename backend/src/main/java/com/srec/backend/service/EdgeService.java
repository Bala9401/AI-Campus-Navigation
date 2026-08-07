package com.srec.backend.service;

import com.srec.backend.entity.Edge;
import com.srec.backend.repository.EdgeRepository;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class EdgeService {

    private final EdgeRepository edgeRepository;

    public EdgeService(EdgeRepository edgeRepository) {
        this.edgeRepository = edgeRepository;
    }

    public List<Edge> getAllEdges() {
        return edgeRepository.findAll();
    }
}