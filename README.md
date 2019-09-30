# PNGParvus Readme
## Description
**PNGParvus** is C# implementation of [svpng](https://github.com/miloyip/svpng), which is a minimalistic PNG encoder.
## Usage
1. Define a `Color` stuct. Names of struct and fields are free to take, it's size of struct that matters.
    ```CSharp
    struct Color
    {
        byte R, G, B, A;    //A is optional
    }
    ```
2. Implement the interface `IPNG<Color>`.
3. Invoke 
    ```CSharp
    Write<TPNG, TColor>(ReadOnlySpan<char> path, TPNG png)
    ```
    or
    ```CSharp
    Write<TPNG, TColor, TStream>(TStream stream, TPNG png)
    ```
### Other Usage
Generic type parameter `TStream` allows any class inheriting form `Stream`, thus you can output png to other streams, for example:
```CSharp
PNGParvus.Write<.., .., System.Stream>(Console.OpenStandardOutput(), ..);
```
This will output the png to console.

## Example
Run the `PNGParvusTest` project, and a 256 * 256 PNG `test.png` will be created at the folder where the compiled files locate, showing elapsed milliseconds.