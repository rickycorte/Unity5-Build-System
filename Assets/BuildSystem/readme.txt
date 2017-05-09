If you have questions and/or you want to submit a bug report contact me at:
- Twitter: @rickycortef
- Email rickycortef@gmail.com

FAQ:
- Why docs are bundled in a .zip file?
That's because the docs use some js files that unity tries to compile.
It's recommend to extract the docs in folder outside Assets/. 
The documentation can be also found at: https://rickycorte.github.io/BuildSystem/manual/index.html

Whas is new in 2.0.0:
- Added skinned mesh support
- Added tool to set a ghost material to all items
- Added automatic item flelds fill to help users to improve build item creation time
- Moved ghost creation in editor
- Removed ghost creation from runtime to gain better performance (less cpu and ram usage)
- Removed some public apis, this breaks compatibility with code written for v1.0