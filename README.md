# covid19ch.github.io

This is a toy project to experiment with two technologies: Blazor webassembly and roslyn source generators.
As a proof of concept, this project uses a source generator to transform data read from spreadsheets, and displays those results in a blazor webassembly application.

## known issues

There are a few issues in this project, that I won't fix anytime soon, simply because the point of the project isn't to serve the public, but to experiment with the above mentioned technologies.

- Since the data is compiled into a dll in compressed form, both downloading the dll and decompressing the data takes a considerable amount of time leading to a very slow page load.
- The calculation for how long it takes until the entire population is affected is wrong, when the current trend is decreasing.
- The calculation for how long it takes until the entire population is affected can exceed the range of `System.DateTime`, when the linear growth assumption is low.
