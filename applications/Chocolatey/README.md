# Chocolatey

## Instructions

To create a new release for chocolatye follow these instructions

1. Update the version numbers & release notes in `iishosts.nuspec`
1. From the command line run `choco pack`
1. Upload the file to Chocolatey

## Notes
- The automated testing will fail. YOu may need to add a comment stating: `Package requires the Windows feature 'Web-Server-Mgmt-Tools' to be installed.`



