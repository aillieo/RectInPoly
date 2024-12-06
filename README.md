## Largest Rectangle in Polygon

This Unity package provides an implementation of the algorithm described in [Algorithm for finding the largest inscribed rectangle in
polygon](https://journals.ut.ac.ir/article_71280_2a21de484e568a9e396458a5930ca06a.pdf) to compute the largest inscribed rectangle within a polygon. This can significantly optimize broadphase calculations for point-in-polygon checks, especially for complex polygons with many vertices.

### Installation

RectInPoly can be installed via the Unity Package Manager. Alternatively, you can clone the repository and import the package manually.

####  Via the Unity Package Manager

<details><summary>Steps for installing via the Unity Package Manager</summary>

- Open the Unity Package Manager from the Window menu.

- Click the + button in the top left corner and select "Add package from git URL".

- Enter the following URL: `https://github.com/aillieo/RectInPoly.git#upm`

- Click the "Add" button to add the package to your project.</details>

#### Manual Installation

<details><summary>Steps for manual Installation</summary>

- Clone the repository to your local machine.

- Open your Unity project and navigate to the "Packages" folder.

- Drag the "RectInPoly" folder from the cloned repository into the "Packages" folder.

- Unity will import the package automatically.</details>

### Usage
To compute the largest inscribed rectangle, use the following code:

```csharp
using UnityEngine;
using System.Collections.Generic;

var polygon = new List<Vector2> {
    new Vector2(0, 0),
    new Vector2(4, 0),
    new Vector2(3, 3),
    new Vector2(0, 3)
};

Rect rect = LargestRectInPolygon.Find(polygon);
Debug.Log($"Largest Inscribed Rectangle: {rect}");
```

### Advanced Usage

In some cases (e.g., when the polygon has very few vertices or uneven vertex distribution), the default subdivide method may not yield optimal results. To handle this, `LargestRectInPolygon.Find` has an overload that allows you to specify a custom subdivision method, see the sample case for details. 

### Samples

An interactive web build is available for real-time visualization and testing. [Click here](https://aillieo.github.io/RectInPoly/) to have a look.

### License

This package is available under the MIT License.

### Reference

https://journals.ut.ac.ir/article_71280_2a21de484e568a9e396458a5930ca06a.pdf
