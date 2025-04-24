extends Node3D

@export var offset_distance: float = 2.0     # Distance behind the ball
@export var height_offset: float = 0.7       # Height above the ball
@export var smooth_follow: bool = true       # Enable for smoother camera movement
@export var smooth_speed: float = 5.0        # Higher = faster camera catch-up

var last_velocity = Vector3.ZERO

func _ready():
	# Initialize position
	var ball = get_parent()
	update_position(ball)

func _physics_process(delta):
	var ball = get_parent()
	
	# Get the ball's velocity on the horizontal plane (x and z)
	var velocity = ball.linear_velocity
	var horizontal_velocity = Vector3(velocity.x, 0, velocity.z)
	
	# Only update if the ball is moving significantly
	if horizontal_velocity.length() > 0.1:
		last_velocity = -horizontal_velocity
	
	# Position the camera based on the rolling direction
	update_position(ball, delta)

func update_position(ball, delta = 0):
	var target_position
	
	# If we have meaningful velocity, position opposite to it
	if last_velocity.length() > 0.1:
		# Get direction on XZ plane only
		var direction = -last_velocity.normalized()
		
		# Calculate target position
		target_position = ball.global_position + (direction * offset_distance)
		target_position.y = ball.global_position.y + height_offset
	else:
		# Fallback if ball isn't moving - position behind ball
		target_position = ball.global_position - Vector3(0, -height_offset, offset_distance)
	
	# Apply smooth movement if enabled
	if smooth_follow and delta > 0:
		global_position = global_position.lerp(target_position, smooth_speed * delta)
	else:
		global_position = target_position
	
	# Always look at the ball, but ONLY rotate around Y axis
	look_at_ball_xz(ball.global_position)

func look_at_ball_xz(target_pos):
	# Get direction to target on XZ plane
	var direction = target_pos - global_position
	direction.y = 0  # Zero out Y component to keep camera level
	
	if direction.length() > 0.001:
		# Only set rotation on Y axis (looking left/right)
		var y_rotation = atan2(direction.x, direction.z)
		
		# Create a new basis with only Y rotation
		var new_basis = Basis(Vector3.UP, y_rotation)
		global_transform.basis = new_basis
