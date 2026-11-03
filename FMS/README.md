# FMS

## PlantUML diagrams

The workspace recommends the PlantUML extension for VS Code and configures it to
render diagrams locally. Local rendering requires a Java runtime; Graphviz is
provided by the extension.

On Windows, install the Eclipse Temurin 21 JDK with:

```powershell
winget install --id EclipseAdoptium.Temurin.21.JDK --exact
```

Restart VS Code after installation and verify Java is available with
`java -version`. Open `current-design.puml` and run **PlantUML: Preview Current
Diagram** from the Command Palette.