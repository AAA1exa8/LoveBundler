# LoveBundler

LoveBundler is CLI version of bundler tool from Love Potion.

## Installation

First install [MSYS2](https://www.msys2.org/) if you are on Windows (skip this step if on Linux) then follow the instructions below:
## Installing devKitPro development toolkits

### GUI aproach

For a easier aproach if on Windows, use the [GUI tool](https://github.com/devkitPro/installer/releases) and select 3DS Development, Wii Development and Switch Development during the compoment setup which will get most of the utilites installed, then launch MSYS2 and run this command to get wut-tools installed:
```bash
sudo (dkp-)pacman -S wut-tools
```
### CLI aproach (traditional method)
To get devKitPro's pacman repositories refer to the [devkitPro wiki](https://devkitpro.org/wiki/Getting_Started).

Then after installing the devKirPro repositories run these commands on MSYS2:
```bash
sudo (dkp-)pacman -Syu && sudo (dkp-)pacman -S switch-dev 3ds-dev wiiu-dev wut-tools
```

## Usage

```
Description:
  LoveBundler, CLI version of bundler for LovePotion

Usage:
  LoveBundler [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  convert <files>  Convert media files to format usable on console
  bundle <dir>     Bundle the game for the specified console
```
