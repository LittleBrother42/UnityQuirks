WindyTerrain

This sample is designed to show how to apply wind effects to custom trees in Unity.

No custom C# scripts are used by this scene; all tree movement is defined by the "Tree Wind.shadergraph" file. The steps are described below (taken from the blog post):

  - Create a wave function (cosine in our case) and random noise function that change over time, and add them together. This gives us a movement pattern that is random (thanks to the noise function), but still follows a somewhat regular pattern that simulates smooth changes in wind strength (thanks to the wave function).

  - Pass this into a separate function that simulates "bending" by strengthening the effect at the higher points on the tree, and weakening it at lower points. This simulates how the tops of trees will be moved more by wind, since higher parts are also affected by wind hitting lower parts.

  - We then apply this modification to each vertex of the tree.

When applied to all materials on the tree, this creates a swaying effect that looks (sort of) like wind belowing through the trees. The main limitation of this effect is that all objects using the shader will move at the same rate, due to the shared random seed. Experimentation is encouraged; for example, if improved realism is required, try modifying the shader to change the swaying strength using a random modifier over time that is based on world position. If done intelligently, this could be used to simulate a gust of wind moving through the landscape!