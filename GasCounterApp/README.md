# Gas Counter Tracking App

A .NET MAUI Android application for tracking and managing gas counter inspections in the field.

## Features

### Core Functionality
- **Offline Map View**: OpenStreetMap integration with Mapsui for offline map support
- **GPS Location Tracking**: High-accuracy GPS tracking with real-time accuracy display
- **Gas Counter Management**: Add, view, modify, and delete gas counter records
- **Check Status Tracking**: Mark counters as checked/unchecked during inspections
- **Search Functionality**: Search counters by ID
- **Inspection Cycle Management**: Track and reset inspection cycles

### Data Management
- **SQLite Database**: Local storage for all counter data
- **Automatic Backups**: Daily automatic backups of the database
- **Manual Backup/Restore**: Create and restore backups on demand
- **Data Persistence**: All data stored locally on device

### User Interface
- **Georgian Language**: All UI text in Georgian
- **Map-Based Navigation**: Visual representation of all counters on a map
- **Dashboard**: Statistics, search, and management features
- **Counter Details**: Full information view and edit capability

## Project Structure

```
GasCounterApp/
├── Models/
│   └── GasCounter.cs           # Gas counter data model
├── Services/
│   ├── DatabaseService.cs      # SQLite database operations
│   ├── BackupService.cs        # Backup and restore functionality
│   └── LocationService.cs      # GPS location services
├── Views/
│   ├── MapPage.xaml/cs         # Main map view
│   ├── DashboardPage.xaml/cs   # Dashboard and management
│   ├── AddCounterPage.xaml/cs  # Add new counter
│   └── CounterDetailPage.xaml/cs # View/edit counter details
├── Platforms/
│   └── Android/
│       └── AndroidManifest.xml # Android permissions
├── Resources/
│   ├── AppIcon/
│   ├── Splash/
│   ├── Fonts/
│   └── Images/
├── App.xaml/cs                 # Application entry point
├── MauiProgram.cs              # Service registration
└── GasCounterApp.csproj        # Project configuration
```

## Technology Stack

- **.NET 9.0**: Latest .NET framework
- **.NET MAUI**: Cross-platform UI framework (Android-only build)
- **SQLite**: Local database (sqlite-net-pcl)
- **Mapsui**: Map rendering library with OpenStreetMap
- **C#**: Primary programming language

## NuGet Packages

- `Microsoft.Maui.Controls` (9.0.111)
- `sqlite-net-pcl` (1.9.172)
- `SQLitePCLRaw.bundle_green` (2.1.10)
- `Mapsui` (5.0.0)
- `Mapsui.Maui` (5.0.0)

## Building the Project

### Prerequisites
1. .NET 9 SDK installed
2. .NET MAUI Android workload installed:
   ```bash
   dotnet workload install maui-android
   ```
3. Android SDK (API 21+) for Android 5.0 and above

### Build Commands

```bash
# Navigate to project directory
cd GasCounterApp

# Restore packages
dotnet restore

# Build for Android
dotnet build -f net9.0-android

# Build in Release mode
dotnet build -f net9.0-android -c Release

# Create APK (requires Android SDK)
dotnet publish -f net9.0-android -c Release
```

## Installation on Device

1. Build the project in Release mode
2. Locate the APK in `bin/Release/net9.0-android/publish/`
3. Transfer APK to Android device
4. Enable "Install from Unknown Sources" in device settings
5. Install the APK

## Data Model

### GasCounter
- **Id**: Auto-increment primary key
- **CounterId**: Optional counter identifier (string)
- **CustomerName**: Optional customer name
- **StreetName**: Optional street name
- **Latitude**: GPS latitude (required)
- **Longitude**: GPS longitude (required)
- **GpsAccuracy**: GPS accuracy in meters
- **State**: Counter condition (Normal, Leaking, Needs Repair, etc.)
- **Notes**: Free-text notes
- **IsChecked**: Check status (boolean)
- **LastCheckedDate**: DateTime of last check
- **CreatedDate**: DateTime of creation
- **ModifiedDate**: DateTime of last modification

## Permissions

The app requires the following Android permissions:

- **Location**: `ACCESS_FINE_LOCATION`, `ACCESS_COARSE_LOCATION`
- **Internet**: For downloading map tiles
- **Storage**: For database backups (API 21-32)
- **Media**: For Android 13+ storage access

## Usage Guide

### Adding a Counter
1. Open the app (starts on Map page)
2. Tap the ➕ button in the bottom action bar
3. Ensure GPS has good accuracy
4. Fill in optional fields (ID, Customer Name, Street Name, State, Notes)
5. Tap "შენახვა" (Save)

### Viewing Counters on Map
- Red pins: Unchecked counters
- Green pins: Checked counters
- Tap 📍 to center map on your current location

### Marking Counter as Checked
**Method 1 - From Dashboard:**
1. Tap ☰ to open Dashboard
2. Search for counter by ID
3. Select counter from results
4. Toggle the "შემოწმების სტატუსი" switch

**Method 2 - Quick note:**
Currently tap events on individual pins show an info message. Full tap-to-check functionality requires additional implementation.

### Search
1. Go to Dashboard (☰ button)
2. Enter Counter ID in search box
3. Select counter from results

### Managing Inspection Cycles
1. Go to Dashboard
2. View current cycle statistics (checked/total)
3. When all counters checked, system prompts to start new cycle
4. Or manually tap "ახალი ციკლის დაწყება" to reset all checks

### Backups
- **Automatic**: Creates daily backup automatically
- **Manual Backup**: Dashboard → "კოპირება" button
- **Restore**: Dashboard → "კოპიების ნახვა" → Select backup → Confirm

## Configuration

### Database Location
```
FileSystem.AppDataDirectory/gascounters.db3
```

### Backup Location
```
FileSystem.AppDataDirectory/Backups/gascounters_backup_YYYYMMDD_HHMMSS.db3
```

### Map Settings
- Initial Center: Tbilisi, Georgia (44.8271° E, 41.7151° N)
- Tile Source: OpenStreetMap
- Zoom Level: 14

## Known Limitations

1. **Map Tap Detection**: Current implementation shows info message instead of directly selecting counters. Requires coordinate conversion implementation for pixel-to-world mapping.

2. **Manual Pin Placement**: When GPS is unavailable, the option to manually place pins on the map is presented but requires additional implementation.

3. **Clustering**: Multiple counters at the same location are shown as separate overlapping pins. True clustering visualization needs additional work.

4. **Offline Maps**: While using OpenStreetMap, tiles are cached by the browser but full offline support requires pre-downloading map tiles.

## Future Enhancements

- [ ] Implement proper map tap-to-select functionality
- [ ] Add manual pin placement when GPS unavailable
- [ ] Implement proper marker clustering for multiple counters at same location
- [ ] Add photo attachment capability
- [ ] Export data to CSV/Excel
- [ ] Add filter by state/condition
- [ ] Implement route optimization for inspection planning
- [ ] Add statistics and reporting features
- [ ] Cloud sync capability (optional)

## Troubleshooting

### GPS Not Working
- Ensure location permissions are granted
- Enable GPS in device settings
- Go outdoors for better signal
- Wait for accuracy to improve (<10m recommended)

### Map Not Loading
- Ensure internet connection for first-time tile download
- Check `INTERNET` permission is granted
- Tiles are cached after first load

### Database Issues
- Use backup/restore feature
- Database file: `FileSystem.AppDataDirectory/gascounters.db3`
- Can be accessed with SQLite tools for recovery

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build -f net9.0-android
```

## Development

### Adding New Features
1. Model changes: Update `Models/GasCounter.cs`
2. Database: Modify `Services/DatabaseService.cs`
3. UI: Add/modify XAML files in `Views/`
4. Logic: Implement in code-behind or ViewModels

### Testing
- Deploy to physical Android device (recommended)
- Or use Android emulator with Google Play Services

## License

This is a custom application developed for gas counter inspection workflow. All rights reserved.

## Support

For issues or questions, refer to the project documentation or contact the development team.

---

**Version**: 1.0
**Build Date**: 2025
**Target Platform**: Android API 21+ (Android 5.0+)
