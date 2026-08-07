package com.srec.backend.entity;

import jakarta.persistence.*;
import lombok.Data;

@Entity
@Table(name = "buildings")
@Data
public class Building {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "building_id")
    private Integer buildingId;

    @Column(name = "building_name", nullable = false)
    private String buildingName;

    @Column(name = "building_code", unique = true)
    private String buildingCode;

    @Column(nullable = false)
    private Double latitude;

    @Column(nullable = false)
    private Double longitude;

    @Column(columnDefinition = "TEXT")
    private String description;
}