# Gas Counter App - Development Guide

## Project Overview

A .NET MAUI Android application for tracking gas meter locations and inspections in the field. Built for personnel in Ozurgeti, Georgia to manage gas counter inspections across their service area.

**Developer**: Built with Claude Code
**Date**: November 2025
**Version**: 1.0
**Target Platform**: Android 5.0+ (API 21+)

---

## Table of Contents

1. [Technologies Used](#technologies-used)
2. [Architecture & Project Structure](#architecture--project-structure)
3. [Features Implemented](#features-implemented)
4. [Setup & Installation](#setup--installation)
5. [Development Process](#development-process)
6. [Issues Encountered & Solutions](#issues-encountered--solutions)
7. [Future Development](#future-development)
8. [Testing Guide](#testing-guide)

---

## Technologies Used

### Core Framework
- **.NET 9.0** - Latest .NET SDK
- **.NET MAUI** - Multi-platform App UI framework (Android-only build)
- **C# 12** - Primary programming language
- **XAML** - UI markup language

### Key Libraries & Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.Maui.Controls | 9.0.111 | MAUI UI framework |
| sqlite-net-pcl | 1.9.172 | SQLite database ORM |
| SQLitePCLRaw.bundle_green | 2.1.10 | SQLite platform support |
| Mapsui | 5.0.0 | Core mapping library |
| Mapsui.Maui | 5.0.0 | MAUI integration for Mapsui |
| SkiaSharp.Views.Maui.Controls | 3.119.1 | Required for Mapsui rendering |

### Map Provider
- **OpenStreetMap** - Free, open-source map tiles
- **Mapsui** - Cross-platform map rendering engine
- **SkiaSharp** - 2D graphics rendering engine

### Android Features
- **GPS/Location Services** - `Microsoft.Maui.Essentials.Geolocation`
- **Permissions** - `Microsoft.Maui.Essentials.Permissions`
- **File System** - `Microsoft.Maui.Essentials.FileSystem`

---

## Architecture & Project Structure

### MVVM-Lite Pattern
The app uses a simplified MVVM pattern without a formal ViewModel layer for simplicity:
- **Models** - Data entities
- **Views** - XAML pages + code-behind
- **Services** - Business logic layer

### Project Structure

```
GasCounterApp/
│
├── Models/
│   └── GasCounter.cs                 # Gas counter entity
│
├── Services/
│   ├── DatabaseService.cs            # SQLite CRUD operations
│   ├── LocationService.cs            # GPS location handling
│   └── BackupService.cs              # Database backup/restore
│
├── Views/
│   ├── MapPage.xaml/cs              # Main map view (startup page)
│   ├── AddCounterPage.xaml/cs       # Add new counter form
│   ├── CounterDetailPage.xaml/cs    # View/edit counter details
│   └── DashboardPage.xaml/cs        # Management dashboard
│
├── Resources/
│   ├── Styles/
│   │   ├── Colors.xaml              # Theme colors (light/dark)
│   │   └── Styles.xaml              # UI styles
│   ├── AppIcon/                     # App icon assets
│   ├── Splash/                      # Splash screen
│   ├── Fonts/                       # Custom fonts
│   └── Images/                      # Image assets
│
├── Platforms/
│   └── Android/
│       ├── AndroidManifest.xml      # Permissions & config
│       └── MainActivity.cs          # Android entry point
│
├── App.xaml/cs                      # Application entry
├── MauiProgram.cs                   # Service registration
└── GasCounterApp.csproj             # Project configuration
```

---

## Features Implemented

### 1. Map View (Main Page)
**File**: `Views/MapPage.xaml.cs`

- **OpenStreetMap Integration**
  - Displays map centered on Ozurgeti (42.0041°E, 41.9245°N)
  - Offline-capable (caches downloaded tiles)
  - Pan and zoom controls

- **Counter Visualization**
  - Red pins: Unchecked counters
  - Green pins: Checked counters
  - Clustered display for multiple counters at same location

- **GPS Auto-Center**
  - Automatically centers map on user location when GPS lock achieved (accuracy < 20m)
  - Only auto-centers once per session
  - Manual recenter via 📍 button

- **Real-time GPS Display**
  - Shows current GPS accuracy in Georgian
  - Updates every 3 seconds
  - Color-coded accuracy indicator

- **Bottom Action Bar**
  - 📍 My Location button
  - Statistics display (X/Y checked)
  - ➕ Add Counter button
  - ☰ Dashboard button

### 2. Add Counter Page
**File**: `Views/AddCounterPage.xaml.cs`

- **GPS Location Capture**
  - Automatic GPS coordinate capture
  - Displays accuracy rating
  - Color-coded accuracy (green < 10m, orange < 20m, red > 20m)
  - Fallback to manual pin placement if GPS unavailable

- **Optional Data Fields**
  - Counter ID (text)
  - Customer Name (text)
  - Street Name (text)
  - State/Condition (dropdown: Normal, Leaking, Needs Repair, Damaged, etc.)
  - Notes (multi-line text)

- **Data Validation**
  - Location is required (enforced)
  - All other fields optional

### 3. Counter Detail Page
**File**: `Views/CounterDetailPage.xaml.cs`

- **View/Edit Counter**
  - Display all counter information
  - Edit any field
  - Check/uncheck status with toggle switch
  - Shows last checked date/time

- **Two-Step Deletion**
  - First confirmation: "Are you sure?"
  - Second confirmation: "This is irreversible"
  - Prevents accidental deletion

- **Metadata Display**
  - Created date/time
  - Last modified date/time
  - GPS accuracy at time of creation

### 4. Dashboard Page
**File**: `Views/DashboardPage.xaml.cs`

- **Statistics Panel**
  - Total counters
  - Checked counters (green)
  - Remaining counters (red)
  - Current cycle status

- **Search Functionality**
  - Real-time search by Counter ID
  - Displays results with customer name and street
  - Visual checkmark indicator (✓/✗)

- **Inspection Cycle Management**
  - Manual "Start New Cycle" button
  - Auto-prompt when all counters checked
  - Resets all check statuses
  - Confirmation required

- **Backup Management**
  - Shows last backup date/time
  - Manual "Create Backup" button
  - "View Backups" to see all available backups
  - Restore from backup with confirmation

### 5. Database Layer
**File**: `Services/DatabaseService.cs`

- **SQLite Database**
  - File-based storage: `gascounters.db3`
  - Location: `FileSystem.AppDataDirectory`
  - Async operations for UI responsiveness

- **CRUD Operations**
  - Create counter
  - Read (all, by ID, search by Counter ID)
  - Update counter
  - Delete counter

- **Statistics Queries**
  - Get total count
  - Get checked count
  - Reset all checks

### 6. Location Services
**File**: `Services/LocationService.cs`

- **Permission Handling**
  - Auto-request LocationWhenInUse permission
  - Graceful fallback if permission denied

- **GPS Operations**
  - Get current location (one-time)
  - Continuous location updates (every 2 seconds)
  - Best accuracy mode
  - 30-second timeout for GPS lock

- **Accuracy Descriptions** (in Georgian)
  - < 5m: "ძალიან ზუსტი" (Very accurate)
  - < 10m: "ზუსტი" (Accurate)
  - < 20m: "საშუალო" (Medium)
  - > 20m: "დაბალი" (Low)

### 7. Backup System
**File**: `Services/BackupService.cs`

- **Automatic Daily Backup**
  - Runs on app start
  - Creates backup if last backup > 24 hours old
  - Filename format: `gascounters_backup_YYYYMMDD_HHMMSS.db3`

- **Manual Backup**
  - User-triggered via Dashboard
  - Immediate backup creation

- **Backup Retention**
  - Keeps last 7 backups
  - Auto-deletes older backups

- **Restore Functionality**
  - List all available backups
  - Restore from any backup
  - Temporary backup before restore (rollback capability)

### 8. Theme Support
**Files**: `Resources/Styles/Colors.xaml`, `App.xaml.cs`

- **System Theme Following**
  - Light mode: White backgrounds, dark text
  - Dark mode: Dark backgrounds (#141414), light text
  - Automatic switching based on system settings

- **Theme-Aware Components**
  - Map background
  - Action bar
  - Text colors
  - All UI elements

---

## Setup & Installation

### Prerequisites

1. **.NET 9 SDK**
   ```bash
   dotnet --version  # Should show 9.0.x
   ```

2. **.NET MAUI Workload**
   ```bash
   dotnet workload install maui-android
   ```

3. **Android SDK**
   - Minimum API 21 (Android 5.0)
   - Target API 35 (Android 14)
   - Installed automatically with MAUI workload

4. **ADB (Android Debug Bridge)**
   - For device deployment
   - Download from: https://developer.android.com/tools/releases/platform-tools

### Building the Project

```bash
# Navigate to project directory
cd C:\Users\lmagh\Documents\socar\GasCounterApp

# Restore dependencies
dotnet restore

# Build for Android (Debug)
dotnet build -f net9.0-android

# Build for Android (Release)
dotnet build -f net9.0-android -c Release

# Publish (creates optimized APK)
dotnet publish -f net9.0-android -c Release
```

### Installing on Device via ADB

#### Prerequisites
1. **Enable Developer Options** on your Android device:
   - Go to Settings > About Phone
   - Tap "Build Number" 7 times
   - Developer Options will appear in Settings

2. **Enable USB Debugging**:
   - Go to Settings > Developer Options
   - Enable "USB Debugging"
   - Connect device via USB
   - Accept the "Allow USB debugging" prompt on device

3. **Install ADB Tools** (if not already installed):
   - **Windows**: Download platform-tools from https://developer.android.com/tools/releases/platform-tools
   - **Mac/Linux**: `brew install android-platform-tools` or similar
   - Add ADB to your PATH environment variable

#### Method 1: Direct ADB Installation (Recommended)

**Step 1: Verify Device Connection**
```bash
# List connected devices
adb devices

# Expected output:
# List of devices attached
# ABC123XYZ    device
```

If you see "unauthorized", check your phone for the USB debugging authorization prompt.

**Step 2: Build the APK**
```bash
# Navigate to project root
cd /home/user/gas-counter-app

# Clean previous builds (optional)
dotnet clean

# Build Release APK
dotnet build -f net9.0-android -c Release

# The APK will be created at:
# GasCounterApp/bin/Release/net9.0-android/com.gascounter.app-Signed.apk
```

**Step 3: Install APK**
```bash
# Fresh install (if app not already installed)
adb install "GasCounterApp/bin/Release/net9.0-android/com.gascounter.app-Signed.apk"

# Reinstall (keeps app data - recommended for updates)
adb install -r "GasCounterApp/bin/Release/net9.0-android/com.gascounter.app-Signed.apk"

# Force reinstall (removes app data - clean install)
adb install -r -d "GasCounterApp/bin/Release/net9.0-android/com.gascounter.app-Signed.apk"
```

**Step 4: Launch App**
```bash
# Launch app immediately after installation
adb shell am start -n com.gascounter.app/crc64e39b72f7d7f6e83e.MainActivity

# Or manually tap the "Gas Counter" icon on device
```

#### Method 2: Build and Install in One Command

For convenience, create a script to build and install:

**Windows (PowerShell):**
```powershell
# build-and-install.ps1
dotnet clean
dotnet build -f net9.0-android -c Release
$apkPath = "GasCounterApp\bin\Release\net9.0-android\com.gascounter.app-Signed.apk"
adb install -r $apkPath
adb shell am start -n com.gascounter.app/crc64e39b72f7d7f6e83e.MainActivity
Write-Host "App installed and launched successfully!"
```

**Linux/Mac (Bash):**
```bash
#!/bin/bash
# build-and-install.sh
dotnet clean
dotnet build -f net9.0-android -c Release
APK_PATH="GasCounterApp/bin/Release/net9.0-android/com.gascounter.app-Signed.apk"
adb install -r "$APK_PATH"
adb shell am start -n com.gascounter.app/crc64e39b72f7d7f6e83e.MainActivity
echo "App installed and launched successfully!"
```

#### Method 3: Install Over WiFi (Wireless ADB)

**Step 1: Connect Device via USB first**
```bash
# Enable TCP/IP mode on port 5555
adb tcpip 5555

# Get device IP address
adb shell ip addr show wlan0 | grep "inet "
# Note the IP address (e.g., 192.168.1.100)
```

**Step 2: Disconnect USB and Connect via WiFi**
```bash
# Replace with your device's IP
adb connect 192.168.1.100:5555

# Verify connection
adb devices
# Should show: 192.168.1.100:5555    device

# Now use regular adb commands wirelessly
adb install -r "GasCounterApp/bin/Release/net9.0-android/com.gascounter.app-Signed.apk"
```

**Step 3: Return to USB Mode**
```bash
adb usb
```

#### Method 4: Manual Transfer to Device

If ADB is not available:

1. **Copy APK to Device**:
   - Connect device via USB as file transfer device
   - Copy `com.gascounter.app-Signed.apk` to Downloads folder
   - Or email/cloud share the APK to yourself

2. **Enable Unknown Sources**:
   - Settings > Security > Install Unknown Apps
   - Enable for your file manager or browser

3. **Install from Device**:
   - Open Files/Downloads app
   - Tap the APK file
   - Tap "Install"
   - Tap "Open" to launch

#### Useful ADB Commands

**Device Management:**
```bash
# List all connected devices
adb devices -l

# Get device info
adb shell getprop ro.build.version.release  # Android version
adb shell getprop ro.product.model          # Device model

# Reboot device
adb reboot
```

**App Management:**
```bash
# Check if app is installed
adb shell pm list packages | grep com.gascounter.app

# Get app version
adb shell dumpsys package com.gascounter.app | grep versionName

# Uninstall app
adb uninstall com.gascounter.app

# Uninstall but keep data
adb uninstall -k com.gascounter.app

# Clear app data without uninstalling
adb shell pm clear com.gascounter.app
```

**Logging & Debugging:**
```bash
# View real-time logs (all)
adb logcat

# View only errors
adb logcat *:E

# Filter for your app
adb logcat | grep -i "gascounter"

# Save logs to file
adb logcat -d > logcat.txt

# Clear log buffer
adb logcat -c
```

**File Access:**
```bash
# Access app's internal storage
adb shell run-as com.gascounter.app

# Pull database file to computer (requires root or debug build)
adb shell run-as com.gascounter.app cp /data/user/0/com.gascounter.app/files/gascounters.db3 /sdcard/
adb pull /sdcard/gascounters.db3

# Push database back to device
adb push gascounters.db3 /sdcard/
adb shell run-as com.gascounter.app cp /sdcard/gascounters.db3 /data/user/0/com.gascounter.app/files/
```

**Screenshots & Screen Recording:**
```bash
# Take screenshot
adb shell screencap /sdcard/screenshot.png
adb pull /sdcard/screenshot.png

# Record screen (Ctrl+C to stop)
adb shell screenrecord /sdcard/demo.mp4
adb pull /sdcard/demo.mp4
```

#### Troubleshooting ADB Installation

**Problem: "adb: command not found"**
```bash
# Windows: Add to PATH
setx PATH "%PATH%;C:\path\to\platform-tools"

# Mac/Linux: Add to ~/.bashrc or ~/.zshrc
export PATH="$PATH:/path/to/platform-tools"
```

**Problem: "device unauthorized"**
```bash
# Disconnect and reconnect USB cable
# Check phone for authorization dialog
# If still fails, revoke all USB debugging authorizations in Developer Options
# Then reconnect and reauthorize
```

**Problem: "device offline"**
```bash
adb kill-server
adb start-server
adb devices
```

**Problem: "INSTALL_FAILED_UPDATE_INCOMPATIBLE"**
```bash
# App signature mismatch - uninstall first
adb uninstall com.gascounter.app
adb install "GasCounterApp/bin/Release/net9.0-android/com.gascounter.app-Signed.apk"
```

**Problem: "INSTALL_FAILED_INSUFFICIENT_STORAGE"**
```bash
# Check available space
adb shell df /data

# Free up space or uninstall other apps
adb shell pm list packages
adb uninstall <package.name>
```

**Problem: Multiple devices connected**
```bash
# Specify device by serial number
adb -s ABC123XYZ install -r "path/to/apk"

# Or use USB/TCP to specify
adb -d install -r "path/to/apk"  # USB device
adb -e install -r "path/to/apk"  # Emulator
```

### First Run Setup

1. **Grant Permissions**
   - Location permission will be requested on first GPS use
   - Grant "While using the app" or "Always"

2. **Initial Map Load**
   - App will download OpenStreetMap tiles for Ozurgeti area
   - Requires internet connection for first use
   - Tiles are cached for offline use

3. **Database Initialization**
   - SQLite database created automatically on first run
   - Location: `/data/user/0/com.gascounter.app/files/gascounters.db3`

---

## Development Process

### Phase 1: Project Setup ✅
1. Created .NET MAUI project structure
2. Configured for Android-only build
3. Added NuGet dependencies
4. Set up project folders (Models, Services, Views)

### Phase 2: Data Layer ✅
1. Designed `GasCounter` model with all required fields
2. Implemented `DatabaseService` with async SQLite operations
3. Created `BackupService` for data persistence
4. Tested CRUD operations

### Phase 3: Location Services ✅
1. Implemented `LocationService` with GPS handling
2. Added permission checking and requesting
3. Created accuracy display in Georgian
4. Implemented auto-center on GPS lock

### Phase 4: Map Integration ✅
1. Integrated Mapsui with OpenStreetMap
2. Fixed SkiaSharp dependency issue
3. Implemented counter pin rendering
4. Added color-coding for check status
5. Set default center to Ozurgeti

### Phase 5: UI Pages ✅
1. **MapPage**: Main view with map and action bar
2. **AddCounterPage**: Form for new counters
3. **CounterDetailPage**: View/edit existing counters
4. **DashboardPage**: Management and statistics

### Phase 6: Theme Support ✅
1. Added dark/light theme definitions
2. Implemented system theme following
3. Updated all pages for theme awareness

### Phase 7: Bug Fixes & Testing ✅
1. Fixed Mapsui crash (SkiaSharp handler registration)
2. Fixed navigation null reference issues
3. Added permission handling in LocationService
4. Added error handling and try-catch blocks

---

## Issues Encountered & Solutions

### Issue 1: Mapsui Crash on Launch
**Error**: `HandlerNotFoundException: Unable to find IElementHandler for SKGLView`

**Cause**: Mapsui.Maui requires SkiaSharp handlers to be registered

**Solution**:
```csharp
// In MauiProgram.cs
builder
    .UseMauiApp<App>()
    .UseSkiaSharp()  // Added this line
```

Added NuGet package: `SkiaSharp.Views.Maui.Controls` v3.119.1

**Files Changed**:
- `MauiProgram.cs` - Added `.UseSkiaSharp()`
- `GasCounterApp.csproj` - Added SkiaSharp package reference

---

### Issue 2: App Crash When Tapping Add/Menu Buttons
**Error**: `System.NullReferenceException: Object reference not set to an instance of an object`

**Cause**: Multiple issues:
1. Navigation property was null
2. Geolocation API not called correctly
3. Permissions not requested before GPS access

**Solution**:

**Fix 1 - Added Navigation null check**:
```csharp
if (Navigation != null)
{
    await Navigation.PushAsync(addPage);
}
else
{
    await DisplayAlert("შეცდომა", "ნავიგაცია ვერ მოიძებნა", "OK");
}
```

**Fix 2 - Corrected Geolocation API**:
```csharp
// OLD (incorrect)
var location = await Geolocation.GetLocationAsync(request);

// NEW (correct)
var location = await Geolocation.Default.GetLocationAsync(request);
```

**Fix 3 - Added Permission Handling**:
```csharp
var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
if (status != PermissionStatus.Granted)
{
    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
    if (status != PermissionStatus.Granted)
    {
        return (null, null);
    }
}
```

**Files Changed**:
- `Views/MapPage.xaml.cs` - Added null checks and try-catch
- `Services/LocationService.cs` - Added permission checking and API fix

---

### Issue 3: MAUI Workload Installation Failed
**Error**: `Downloading microsoft.android.sdk.windows.msi.x64 (35.0.105) failed`

**Solution**:
```bash
# Update workloads first
dotnet workload update

# Then install
dotnet workload install maui-android
```

The update fixed package repository issues.

---

### Issue 4: Dark Theme Colors Not Working
**Error**: `XC0009: No property 'Light' found for Color`

**Cause**: `Color` type doesn't support `Light` and `Dark` attributes directly in XAML

**Solution**: Use `AppThemeBinding` in XAML:
```xml
<!-- OLD (incorrect) -->
<Color x:Key="SurfaceColor" Light="{StaticResource White}" Dark="{StaticResource Gray950}"/>

<!-- NEW (correct) -->
<Color x:Key="SurfaceColor">#FFFFFF</Color>
<Color x:Key="SurfaceColorDark">#141414</Color>

<!-- In usage -->
BackgroundColor="{AppThemeBinding Light={StaticResource SurfaceColor}, Dark={StaticResource SurfaceColorDark}}"
```

**Files Changed**:
- `Resources/Styles/Colors.xaml` - Separate light/dark color definitions
- `Views/MapPage.xaml` - Use AppThemeBinding

---

## Future Development

### High Priority (Needed for Production)

1. **Map Pin Tap Detection** ⚠️
   - Current: Shows info message on map tap
   - Needed: Detect which pin was tapped and show counter popup
   - Implementation: Requires coordinate conversion (screen to world coordinates)

2. **Manual Pin Placement** ⚠️
   - Current: Option shown but not implemented
   - Needed: Allow user to tap map to place pin when GPS unavailable
   - Implementation: Capture tap coordinates and convert to lat/long

3. **Pin Clustering** 🔧
   - Current: Overlapping pins at same location
   - Needed: Visual clustering with counter badge
   - Implementation: Implement Mapsui clustering or custom solution

### Medium Priority (UX Improvements)

4. **Offline Map Pre-Download**
   - Allow users to download map tiles for entire service area
   - Reduces mobile data usage
   - Implementation: Use Mapsui tile caching with predefined bounds

5. **Photo Attachments**
   - Add camera/gallery integration
   - Attach photos to counters
   - Store in `FileSystem.AppDataDirectory/Photos/`

6. **Export to Excel/CSV**
   - Export all counter data
   - Useful for reporting
   - Implementation: Use NPOI or ClosedXML library

7. **Filter by State**
   - Dashboard filter: Show only "Leaking" or "Needs Repair"
   - Helps prioritize work

8. **Route Optimization**
   - Plan optimal route through unchecked counters
   - Integration with Google Maps for navigation
   - Implementation: Use traveling salesman algorithm

### Low Priority (Nice to Have)

9. **Cloud Sync**
   - Optional cloud backup/sync
   - Multi-device support
   - Implementation: Azure Mobile Apps or Firebase

10. **Statistics Dashboard**
    - Charts and graphs
    - Check rate over time
    - Counter age analytics

11. **Multi-User Support**
    - Track which user checked which counter
    - User management
    - Implementation: Add `UserId` field to database

12. **Localization Framework**
    - Support multiple languages beyond Georgian
    - Implementation: Use .NET Resources (.resx files)

---

## Testing Guide

### Manual Testing Checklist

#### Map View
- [ ] App launches without crash
- [ ] Map displays correctly (Ozurgeti center)
- [ ] Map can be panned and zoomed
- [ ] GPS accuracy updates in bottom bar
- [ ] Map auto-centers when GPS lock achieved
- [ ] 📍 button re-centers map
- [ ] Dark/light theme switches correctly

#### Add Counter
- [ ] ➕ button opens Add Counter page
- [ ] Current GPS location displays
- [ ] Accuracy color-coded correctly
- [ ] All fields can be filled
- [ ] Counter saves successfully
- [ ] Returns to map after save
- [ ] New pin appears on map

#### Counter Details
- [ ] Tap pin shows quick actions (currently shows info message)
- [ ] Dashboard search finds counters
- [ ] Tap search result opens details
- [ ] All fields can be edited
- [ ] Check toggle works
- [ ] Save updates counter
- [ ] Delete requires two confirmations
- [ ] Delete removes pin from map

#### Dashboard
- [ ] ☰ button opens dashboard
- [ ] Statistics display correctly
- [ ] Search works in real-time
- [ ] "Start New Cycle" resets all checks
- [ ] Backup creation succeeds
- [ ] Last backup date displays
- [ ] Backup list shows available backups
- [ ] Restore backup works

#### Permissions
- [ ] Location permission requested on first GPS use
- [ ] App works after granting permission
- [ ] App handles permission denial gracefully

### Automated Testing (To Be Implemented)

```csharp
// Example unit test structure

[Test]
public async Task DatabaseService_SaveCounter_Success()
{
    var db = new DatabaseService();
    var counter = new GasCounter
    {
        Latitude = 41.9245,
        Longitude = 42.0041,
        CounterId = "TEST001"
    };

    var id = await db.SaveCounterAsync(counter);

    Assert.Greater(id, 0);
}
```

**Recommended Testing Framework**: xUnit or NUnit

---

## Configuration Files

### AndroidManifest.xml
```xml
<!-- Location Permissions -->
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />

<!-- Network for map tiles -->
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />

<!-- Storage for backups (Android 12 and below) -->
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" android:maxSdkVersion="32" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" android:maxSdkVersion="32" />

<!-- Media permissions for Android 13+ -->
<uses-permission android:name="android.permission.READ_MEDIA_IMAGES" />
```

### GasCounterApp.csproj Key Settings
```xml
<PropertyGroup>
    <TargetFrameworks>net9.0-android</TargetFrameworks>
    <ApplicationTitle>Gas Counter</ApplicationTitle>
    <ApplicationId>com.gascounter.app</ApplicationId>
    <ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
    <ApplicationVersion>1</ApplicationVersion>
    <SupportedOSPlatformVersion>21.0</SupportedOSPlatformVersion>
    <AndroidEnableProfiledAot>false</AndroidEnableProfiledAot>
</PropertyGroup>
```

---

## Database Schema

### GasCounter Table

| Column | Type | Attributes | Description |
|--------|------|-----------|-------------|
| Id | INTEGER | PRIMARY KEY, AUTOINCREMENT | Unique identifier |
| CounterId | TEXT | INDEXED, NULL | Counter ID (optional) |
| CustomerName | TEXT | NULL | Customer name (optional) |
| StreetName | TEXT | NULL | Street name (optional) |
| Latitude | REAL | NOT NULL | GPS latitude |
| Longitude | REAL | NOT NULL | GPS longitude |
| GpsAccuracy | REAL | NULL | GPS accuracy in meters |
| State | TEXT | NULL | Counter condition |
| Notes | TEXT | NULL | Free-text notes |
| IsChecked | BOOLEAN | NOT NULL | Check status |
| LastCheckedDate | DATETIME | NULL | Last check timestamp |
| CreatedDate | DATETIME | NOT NULL | Creation timestamp |
| ModifiedDate | DATETIME | NOT NULL | Last modification timestamp |

**Indexes**:
- `CounterId` - For search performance

---

## Performance Considerations

### Database
- All operations are async to avoid UI blocking
- Indexes on frequently searched fields
- Transaction batching for bulk operations

### Map
- Tile caching for offline use
- Limited number of visible pins (consider clustering)
- Lazy loading of counter details

### GPS
- Polling interval: 3 seconds (balance between accuracy and battery)
- Timeout: 30 seconds for initial lock
- Best accuracy mode only when needed

### Memory
- Dispose of services properly
- Clear old backups (keep only 7)
- Limit image sizes if photo feature added

---

## Deployment Checklist

### Pre-Deployment
- [ ] Test on multiple Android devices
- [ ] Test on Android 13+ and Android 5.0
- [ ] Verify GPS accuracy in field
- [ ] Test with 1000+ counters loaded
- [ ] Test backup/restore functionality
- [ ] Verify offline map caching works
- [ ] Check battery usage over full inspection cycle
- [ ] Test in dark mode

### APK Signing
```bash
# For production, use a keystore
dotnet publish -f net9.0-android -c Release /p:AndroidKeyStore=true /p:AndroidSigningKeyStore=myapp.keystore
```

### Distribution
- Manual installation via USB/SD card
- Internal company file server
- Google Play Store (optional, requires developer account)

---

## Troubleshooting

### GPS Not Working
1. Check location permission granted
2. Enable GPS in device settings
3. Go outdoors for better signal
4. Wait 30 seconds for GPS lock

### Map Not Loading
1. Check internet connection (first time)
2. Verify INTERNET permission in manifest
3. Clear app data and reinstall
4. Check if OpenStreetMap is accessible

### Database Corruption
1. Go to Dashboard > View Backups
2. Restore from most recent backup
3. If no backups, reinstall app (data will be lost)

### App Crashes on Launch
1. Check Android version (must be 5.0+)
2. Clear app cache
3. Reinstall app
4. Check logcat for errors: `adb logcat *:E`

---

## Glossary

**Georgian Terms in App**:
- **რუკა** (ruka) - Map
- **მრიცხველის დამატება** (mritskvelis damateba) - Add Counter
- **შემოწმებული** (shemots'mebuli) - Checked
- **დაფიქსირდა შეცდომა** (dapik'sirda shets'doma) - Error occurred
- **ნავიგაცია** (navigatsia) - Navigation
- **GPS მიუწვდომელია** (GPS miuts'vdomelia) - GPS unavailable
- **ზუსტი** (zusti) - Accurate
- **ძალიან ზუსტი** (dzalian zusti) - Very accurate

---

## Credits & Resources

- **Framework**: [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- **Map Library**: [Mapsui](https://mapsui.com/)
- **Map Data**: [OpenStreetMap](https://www.openstreetmap.org/)
- **Database**: [SQLite](https://www.sqlite.org/)
- **Icons**: Unicode emoji characters

---

## License & Copyright

**Copyright**: 2025 - Gas Counter Tracking System
**License**: Proprietary - Internal Use Only
**Contact**: Development team

---

## Version History

### v1.0 (Current)
- Initial release
- Core functionality complete
- GPS tracking and mapping
- Database with backup
- Dark theme support
- Georgian localization

### Planned v1.1
- Pin tap detection
- Manual pin placement
- Pin clustering
- Photo attachments

---

**Last Updated**: November 12, 2025
**Maintained By**: Development Team
