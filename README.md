# LoveBundler

LoveBundler is CLI version of bundler tool from Love Potion.

## Installation

Install devtools

```bash
sudo (dkp-)pacman -Syu && sudo (dkp-)pacman -S switch-dev 3ds-dev wiiu-dev
```

To get pacman refer to [devkitPro wiki](https://devkitpro.org/wiki/Getting_Started).
If you already have pacman installed you can customize [existing pacman installation](https://devkitpro.org/wiki/devkitPro_pacman#Customising_Existing_Pacman_Install).

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
