extends MeshInstance3D

@export var grid_color: Color = Color(0.2, 0.2, 0.2)
@export var base_color: Color = Color(0.7, 0.7, 0.7)
@export var grid_size: float = 1.0

func _ready():
	# Create a standard material with grid texture
	var material = StandardMaterial3D.new()
	
	# Create a grid texture
	var image = create_grid_texture(32, 32, base_color, grid_color, grid_size)
	var texture = ImageTexture.create_from_image(image)
	
	# Set up material
	material.albedo_color = Color(1, 1, 1)
	material.albedo_texture = texture
	material.uv1_scale = Vector3(10, 10, 10)  # Scale the texture
	
	# Apply material
	material_override = material

func create_grid_texture(width: int, height: int, bg_color: Color, line_color: Color, line_width_percent: float):
	var img = Image.create(width, height, false, Image.FORMAT_RGBA8)
	
	# Fill with background color
	img.fill(bg_color)
	
	# Calculate line width in pixels
	var line_width = int(max(1, width * line_width_percent / 10.0))
	
	# Draw horizontal and vertical lines
	for x in range(width):
		for y in range(height):
			if x < line_width or x > width - line_width - 1:
				if y % (width / 2) < width / 4:
					img.set_pixel(x, y, line_color)
			if y < line_width or y > height - line_width - 1:
				if x % (height / 4) < height / 8:
					img.set_pixel(x, y, line_color)
	
	return img
