# Database Architecture Documentation

## 1. High-Level Architectural Overview
**Technology Stack:** Microsoft SQL Server, ASP.NET Core, Entity Framework (EF) Core.

**Database Justification:**
A relational database architecture was selected for this API because the domain entities possess highly structured, strictly defined relationships that benefit from relational integrity constraints. Specifically:
* A **User** owns many **Playlists** (One-to-Many).
* **Playlists** and **Songs** share a **Many-to-Many** relationship, as a single song can exist across multiple playlists, and a single playlist aggregates many songs.

---

## 2. Entity-Relationship Summary

| Entity 1 | Relationship | Entity 2 | Description |
| :--- | :--- | :--- | :--- |
| **User** | One-to-Many (1:M) | **Playlist** | A single user can create and own multiple playlists. |
| **Playlist** | Many-to-Many (M:M) | **Song** | A playlist contains multiple songs; a song can be added to multiple playlists. |

---

## 3. Data Dictionary

### `Users` Table
Stores the application users who own the playlists.

| Column Name | Data Type | Key Constraints | Description |
| :--- | :--- | :--- | :--- |
| `Id` | `UNIQUEIDENTIFIER` | Primary Key | Uniquely identifies the user. |
| `Name` | `NVARCHAR(MAX)` | | The user's display name. |

### `Playlists` Table
Stores the core playlist data and links it to a specific user.

| Column Name | Data Type | Key Constraints | Description |
| :--- | :--- | :--- | :--- |
| `Id` | `UNIQUEIDENTIFIER` | Primary Key | Uniquely identifies the playlist. |
| `Name` | `NVARCHAR(MAX)` | | The name of the playlist. |
| `UserId` | `UNIQUEIDENTIFIER` | Foreign Key | Maps the playlist to its owner in the `Users` table. |

### `Songs` Table
Stores individual song metadata. *(Note: Referenced as Tracks in the domain logic, mapped as Songs in the database).*

| Column Name | Data Type | Key Constraints | Description |
| :--- | :--- | :--- | :--- |
| `Id` | `UNIQUEIDENTIFIER` | Primary Key | Uniquely identifies the song. |
| `Title` | `NVARCHAR(MAX)` | | The title of the song. |

### `PlaylistSong` Table (Join Table)
An implicit join table generated automatically by EF Core conventions to handle the Many-to-Many relationship between the `Playlists` and `Songs` tables. No explicit entity class is required in the C# domain model for this table.

| Column Name | Data Type | Key Constraints | Description |
| :--- | :--- | :--- | :--- |
| `PlaylistsId` | `UNIQUEIDENTIFIER` | Composite PK, FK | Foreign key mapping to the `Playlists` table. |
| `SongsId` | `UNIQUEIDENTIFIER` | Composite PK, FK | Foreign key mapping to the `Songs` table. |

---

## 4. ORM Integration (EF Core Notes)
* **Implicit Join Table:** The `PlaylistSong` table is managed entirely via EF Core's implicit many-to-many relationship conventions. The application interacts with `Playlist.Songs` and `Song.Playlists` collection navigation properties directly, without manually querying the `PlaylistSong` payload. 
* **Cascading Deletes:** (Standard EF Core behavior to note) Deleting a `Playlist` or a `Song` will automatically remove the corresponding linking records in the `PlaylistSong` table to maintain referential integrity.
