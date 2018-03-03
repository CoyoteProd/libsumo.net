
using cv2;

using logging;

using threading;

using time;

using keyboard = pynput.keyboard;

using SumoController = controller.SumoController;

public static class test {
    
    public static object up = false;
    
    public static object down = false;
    
    public static object left = false;
    
    public static object right = false;
    
    public static object speed = 0;
    
    public static object turn = 0;
    
    public static object ACCELERATION_CONSTANT = 10;
    
    public static object DECCELERATION_CONSTANT = 5;
    
    public static object TURN_CONSTANT = 2;
    
    public static object main() {
        //GLOBAL ctrl
        var ctrl = SumoController();
        ctrl.connect();
        Console.WriteLine("starting...");
        ctrl.volume(0);
        ctrl.requestAllStates();
        // Collect events until released
        using (var listener = keyboard.Listener(on_press: on_press, on_release: on_release)) {
            threading.Timer(0.01, task).start();
            threading.Timer(10, displayBatteryLevel).start();
            listener.join();
        }
    }
    
    public static object displayBatteryLevel() {
        //GLOBAL ctrl
        Console.WriteLine("{}%".format(ctrl.BatteryLevel()));
        threading.Timer(10, displayBatteryLevel).start();
    }
    
    public static object task() {
        object turn;
        object speed;
        //GLOBAL ctrl
        //GLOBAL speed, turn
        var mod = 0;
        if (up == true) {
            if (speed >= 0) {
                mod = ACCELERATION_CONSTANT;
            } else {
                //breaking - we are going reverse 
                mod = ACCELERATION_CONSTANT * 2;
            }
        } else if (down == true) {
            if (speed <= 0) {
                mod = -ACCELERATION_CONSTANT;
            } else {
                //breaking 
                mod = -ACCELERATION_CONSTANT * 2;
            }
        } else {
            mod = -speed / DECCELERATION_CONSTANT;
            ///* the faster we go the more we reduce speed */
            if (mod == 0 && speed) {
                if (speed < 0) {
                    mod = 1;
                } else {
                    mod = -1;
                }
            }
        }
        speed += mod;
        if (speed > 127) {
            speed = 127;
        }
        if (speed < -127) {
            speed = -127;
        }
        ///* turning */        
        mod = 0;
        if (left == true) {
            mod = -TURN_CONSTANT;
        } else if (right == true) {
            mod = TURN_CONSTANT;
        } else {
            mod = -turn / TURN_CONSTANT * 3;
            if (abs(turn) < TURN_CONSTANT && turn) {
                mod = -turn;
            }
        }
        turn += mod;
        if (turn > 32) {
            turn = 32;
        }
        if (turn < -32) {
            turn = -32;
        }
        //print(speed, turn, mod)
        ctrl.move(speed, turn);
        threading.Timer(0.01, task).start();
    }
    
    public static object on_press(object key) {
        //GLOBAL up, down, left, right
        //GLOBAL ctrl
        try {
            Console.WriteLine("alphanumeric key {0} pressed".format(key.char));
            // param = enum[stop, spin, tap, slowshake, metronome, oudulation, spinjump, spintoposture, spiral,slalom]
            if (key.char == "1") {
                ctrl.animation(2);
            } else if (key.char == "2") {
                ctrl.animation(5);
            } else if (key.char == "3") {
                ctrl.animation(3);
            } else if (key.char == "6") {
                ctrl.turn(3);
            }
        } catch (AttributeError) {
            if (key == keyboard.Key.up) {
                var up = true;
            } else if (key == keyboard.Key.down) {
                var down = true;
            } else if (key == keyboard.Key.left) {
                var left = true;
            } else if (key == keyboard.Key.right) {
                var right = true;
            }
        }
    }
    
    public static object on_release(object key) {
        //GLOBAL up, down, left, right
        if (key == keyboard.Key.esc) {
            return false;
        }
        if (key == keyboard.Key.up) {
            var up = false;
        } else if (key == keyboard.Key.down) {
            var down = false;
        } else if (key == keyboard.Key.left) {
            var left = false;
        } else if (key == keyboard.Key.right) {
            var right = false;
        }
    }
    
    static test() {
        logging.basicConfig(filename: "sumo.log", level: logging.INFO);
        main();
    }
}
