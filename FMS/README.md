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

## Google Maps dashboard

The dashboard route map uses the Maps JavaScript API key, while route
calculation uses a separate Routes API key. Configure the browser key as
`GoogleMaps__JavaScriptApiKey` and the server key as `GoogleMaps__ApiKey`
through environment variables or the deployment secret store. The browser key
is returned to the dashboard by `/api/maps/config`, so it is public by design:
restrict it to the deployed website's HTTP referrers and the Maps JavaScript
API. Restrict the server key to the Routes API and never expose it to browsers.

The development mock draws a sample route in central Tokyo. Production routes
require both keys to have the corresponding Google Maps APIs and billing enabled.