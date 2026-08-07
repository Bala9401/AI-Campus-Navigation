package com.srec.backend.entity;

import jakarta.persistence.*;
import lombok.Data;

@Entity
@Table(name = "nodes")
@Data
public class Node {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "node_id")
    private Integer nodeId;

    @Column(name = "floor_id")
    private Integer floorId;

    @Column(name = "node_name")
    private String nodeName;

    private Double latitude;

    private Double longitude;

}