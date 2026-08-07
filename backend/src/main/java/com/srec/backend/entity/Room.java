package com.srec.backend.entity;

import jakarta.persistence.*;
import lombok.Data;

@Entity
@Table(name = "rooms")
@Data
public class Room {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "room_id")
    private Integer roomId;

    @Column(name = "floor_id")
    private Integer floorId;

    @Column(name = "room_number")
    private String roomNumber;

    @Column(name = "room_name")
    private String roomName;

    @Column(name = "room_type")
    private String roomType;

    @Column(name = "latitude")
    private Double latitude;

    @Column(name = "longitude")
    private Double longitude;
}