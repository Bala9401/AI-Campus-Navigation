package com.srec.backend.entity;

import jakarta.persistence.*;
import lombok.Data;

@Entity
@Table(name = "edges")
@Data
public class Edge {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "edge_id")
    private Integer edgeId;

    @Column(name = "from_node")
    private Integer fromNode;

    @Column(name = "to_node")
    private Integer toNode;

    private Double distance;
}