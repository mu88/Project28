﻿using System;

namespace DTO.Location;

public record ExistingLocation(double Latitude, double Longitude, Guid Id, Guid CreatedBy);