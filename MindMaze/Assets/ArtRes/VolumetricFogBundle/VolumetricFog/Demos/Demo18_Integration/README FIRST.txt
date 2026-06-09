This scene has two example of transaprent shaders that have the fog effect integrated:
- Unlit transparent shader with integrated fog
- Standard surface shader (transparent) with integrated fog

The nice thing about this integration is that you can configure the fog to render before transparent shaders and
let the fog also show on desired transparent shaders.
This solves the issue of fog overwriting transparent objects because transparent shaders do not write to depth buffer.

