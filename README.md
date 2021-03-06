# 2D-Shooter
This is a topdown shooter prototype with the main focus on the enemy AI. This is intended as a demonstration/exploration of some concepts.

The AI positions itself based on a grid of surrounding cells, for which a cost is calculated. This calculation is based on:
- Distance to move
- Occupation of the cell
- Distance to allies
- Distance to enemies
- Line of sight
- Perlin noise

The line of sight calculation favors direct line of sight to enemies with cover nearby if the AI is more healthy than its enemies and has its weapon reloaded. Otherwise it prefers cover. This results in interesting and challenging AI behaviour, as it seems to work together in a team and uses cover intelligently.

The AI also tries to shoot into its enemies path instead of directly at them, while considering objects that the target might run into. Targets are selected based on proximity.

Movement is based on physics and an overcomplicated control system.

# What it looks like
Dark blue squares have a higher calculated benefit than light blue squares for the one selected AI and the darkest blue square is picked as the target location.

When the blue player is healthier, the red enemies prefer cover:
![When the blue player is healthier, the red enemies prefer cover](Media/enemy_low.gif?raw=true)

When the red enemies are healthier than the blue player, the enemies prefer moving to spots that provide direct line of sight while being close to cover:
![When the red enemies are healthier than the blue player, the enemies prefer moving to spots that provide direct line of sight while being close to cover](Media/player_low.gif?raw=true)
