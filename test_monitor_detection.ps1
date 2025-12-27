# PowerShell script to test monitor detection
Add-Type -AssemblyName System.Windows.Forms
$screens = [System.Windows.Forms.Screen]::AllScreens
for ($i = 0; $i -lt $screens.Count; $i++) {
    $screen = $screens[$i]
    $bounds = $screen.Bounds
    $area = $bounds.Width * $bounds.Height
    Write-Host "Monitor $i`: $($bounds.Width)x$($bounds.Height) @ ($($bounds.Left),$($bounds.Top)) - Area: $area - Primary: $($screen.Primary)"
}
