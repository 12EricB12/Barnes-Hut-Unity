# Barnes-Hut-Unity
An N-Body simulation running the Barnes Hut algorithm in unity, using rigid bodies to simulate particles.  

# Capibilities
- Can handle up to 10,000 particles  
- O(N lg N) runtime compared to O(N^2) for the usual approach  
- Iterative insertion of particles in the octree instead of recursive, allowing for less memory use

# Limitations
The particles being rigid bodies are the biggest performance bottleneck by far. In the future, DOTS will be used to solve this problem.

# How to use
Currently, there's no front-end or executable, so the only way to run the code is by forking this repo and running it on your system. Ensure you have the most recent version of Unity and Visual Studio if you want to edit the code.
