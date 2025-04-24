extends RigidBody3D

@export var tilt_sensitivity: float = 100.0
@export var max_speed: float = 15.0
@export var calibrate_on_start: bool = true

var gravity_calibration := Vector3.ZERO
var is_calibrated := false

func _ready():
	physics_material_override = PhysicsMaterial.new()
	physics_material_override.friction = 0.7
	physics_material_override.bounce = 0.2
	linear_damp = 0.5
	
	if calibrate_on_start:
		call_deferred("calibrate_sensor")

func calibrate_sensor():
	await get_tree().create_timer(1.0).timeout
	
	# Use get_accelerometer in Godot 4.0.3
	gravity_calibration = Input.get_accelerometer()
	is_calibrated = true
	print("Calibrated accelerometer: ", gravity_calibration)

func _physics_process(delta):
	var direction = Vector3.ZERO
	
	# Check if running on mobile (Android or iOS)
	if OS.has_feature("mobile"):

		var acceleration = Input.get_accelerometer()
		
		if is_calibrated:
			acceleration -= gravity_calibration
		
		direction.x = acceleration.x  # tilt left/right
		direction.z = -acceleration.y   # tilt forward/back
	else:
		# Keyboard input fallback for desktop testing
		if Input.is_action_pressed("ui_right"):
			direction.x += 1
		if Input.is_action_pressed("ui_left"):
			direction.x -= 1
		if Input.is_action_pressed("ui_down"):
			direction.z += 1
		if Input.is_action_pressed("ui_up"):
			direction.z -= 1
		
		# Only normalize if there's input
		if direction.length() > 0:
			direction = direction.normalized()
	
	# Apply force based on direction
	apply_central_force(direction * tilt_sensitivity)
	
	# Cap max speed
	var horizontal_velocity = Vector3(linear_velocity.x, 0, linear_velocity.z)
	if horizontal_velocity.length() > max_speed:
		var limited_velocity = horizontal_velocity.normalized() * max_speed
		linear_velocity.x = limited_velocity.x
		linear_velocity.z = limited_velocity.z
	
	# Debug - use get_accelerometer
	if Input.is_action_just_pressed("ui_accept"):
		print("Accelerometer: ", Input.get_accelerometer())
		print("Calibration: ", gravity_calibration)
		print("Velocity: ", linear_velocity)

func recalibrate():
	calibrate_sensor()
