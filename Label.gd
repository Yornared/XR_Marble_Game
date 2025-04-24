extends Label

# Show this overlay only during development
@export var enabled: bool = true

func _ready():
	visible = enabled

func _process(_delta):
	if not visible:
		return
		
	var acc = Input.get_accelerometer()
	text = "Accelerometer:\n"
	text += "X: %.2f\n" % acc.x
	text += "Y: %.2f\n" % acc.y
	text += "Z: %.2f" % acc.z
