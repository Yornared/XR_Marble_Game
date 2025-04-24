shader_type spatial;

// Grid properties
uniform vec3 grid_color : source_color = vec3(0.2, 0.2, 0.2);
uniform vec3 base_color : source_color = vec3(0.7, 0.7, 0.7);
uniform float line_thickness : hint_range(0.001, 0.1) = 0.02;
uniform float grid_size : hint_range(0.1, 10.0) = 1.0;

void fragment() {
	// Get world position
	vec3 world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	
	// Calculate grid lines in X and Z directions
	float x_line = abs(fract(world_pos.x / grid_size) - 0.5);
	float z_line = abs(fract(world_pos.z / grid_size) - 0.5);
	
	x_line = smoothstep(0.5 - line_thickness, 0.5, x_line);
	z_line = smoothstep(0.5 - line_thickness, 0.5, z_line);
	
	// Combine the lines
	float grid_value = min(x_line, z_line);
	
	// Mix base color with grid color
	vec3 final_color = mix(grid_color, base_color, grid_value);
	
	// Set the final color
	ALBEDO = final_color;
}