# DotNETCHIP-8Emulator

DotNETCHIP-8Emulator is a Chip8 emulator written in .NET 7. It allows you to run and play games designed for the Chip8 virtual machine. This repository includes a Blazor Server project that utilizes the emulation core, which is a .NET class library.

## Getting Started

To get started with the DotNETCHIP-8Emulator, follow these steps:

1. Clone the repository to your local machine

2. Open a terminal and navigate to the BlazorExample folder.

3. Run the emulator:

   ```
   dotnet run
   ```

4. Load a Chip8 ROM file by selecting "Load ROM" from the emulator's menu.

5. Play the game using the keyboard keys mapped to the Chip8 keypad.

## Controls

The DotNETCHIP-8Emulator maps the original Chip8 keypad to the following keyboard keys:

```
Original Keypad             Mapped Keys
+-+-+-+-+                  +-+-+-+-+
|1|2|3|C|                  |1|2|3|4|
+-+-+-+-+    ===>          +-+-+-+-+
|4|5|6|D|                  |Q|W|E|R|
+-+-+-+-+                  +-+-+-+-+
|7|8|9|E|                  |A|S|D|F|
+-+-+-+-+                  +-+-+-+-+
|A|0|B|F|                  |Z|X|C|V|
+-+-+-+-+                  +-+-+-+-+
```

## Features

The DotNETCHIP-8Emulator provides the following features:

- Emulation of the Chip8 virtual machine.
- Rendering of graphics on the screen.
- Sound emulation.
- Keypad input handling.

## Troubleshooting

If you encounter any issues while running the DotNETCHIP-8Emulator, try the following:

- Ensure that you have the .NET 7 SDK installed on your machine.
- Make sure you have the necessary dependencies installed.
- Check that the ROM file you're trying to load is valid and compatible with the Chip8 emulator.

Please note that extensions such as CHIP-10, CHIP-8 HiRes, CHIP-8C, and CHIP-8X are not taken into account in this implementation. ROMs built for these extensions may not work or display undefined behavior.

If the problem persists, feel free to open an issue on the project's GitHub repository.


## Continuous Integration and Deployment
This project includes GitHub Actions configuration for continuous integration and deployment (CI/CD). 
The configured workflow automatically builds and executes unit tests for the DotNETCHIP-8Emulator on each push and pull request to the repository

## License

This project is licensed under the [MIT License](LICENSE). Feel free to use and modify the code according to the terms of the license.

## Acknowledgements

The DotNETCHIP-8Emulator is made possible due to the extensive Wikipedia description written by contributors in the Chip8 community. Special thanks to all the developers who have shared their knowledge.

## References

- [CHIP-8 - Wikipedia](https://en.wikipedia.org/wiki/CHIP-8)
- [CHIP-8 ROMs by kripod](https://github.com/kripod/chip8-roms/)
