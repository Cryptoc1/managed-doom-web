export function createUserInput(element: HTMLElement) {
  const state: UserInputState = {
    delta: { x: 0, y: 0 },
    now: 0,
    pressed: new Set<string>(),
    released: new Set<string>(),
  };

  const onPointerLockChange = () => {
    if (document.pointerLockElement !== element) {
      state.controller?.abort();
      return;
    }

    clearState(state);
    state.controller = createAbortController(() => {
      if (document.pointerLockElement === element) {
        document.exitPointerLock();
      }
    });

    const onKeyboardInput = (e: KeyboardEvent) => {
      if (state.controller?.signal.aborted) {
        return;
      }

      e.preventDefault();
      switch (e.type) {
        case 'keydown': return state.pressed.add(e.code);
        case 'keyup': return state.released.add(e.code);
      }
    };

    const onMouseInput = (e: MouseEvent) => {
      if (state.controller?.signal.aborted) {
        return;
      }

      if (e.type === 'mousemove') {
        state.delta.x += e.movementX;
        state.delta.y += e.movementY;

        return;
      }

      e.preventDefault();
      const map = (button: number) => {
        switch (button) {
          case 0: return 'MouseLeft';
          case 1: return 'MouseMiddle';
          case 2: return 'MouseRight';
          case 3: return 'MouseBack';
          case 4: return 'MouseForward';
          default: throw new TypeError(`Unknown MouseButton: ${button}`);
        }
      };

      const button = map(e.button);
      switch (e.type) {
        case 'mousedown': return state.pressed.add(button);
        case 'mouseup': return state.released.add(button);
      }
    };

    element.addEventListener('keydown', onKeyboardInput, {
      capture: true,
      passive: false,
      signal: state.controller.signal
    });

    element.addEventListener('keyup', onKeyboardInput, {
      capture: true,
      passive: false,
      signal: state.controller.signal
    });

    element.addEventListener('mousedown', onMouseInput, {
      capture: true,
      passive: false,
      signal: state.controller.signal
    });

    element.addEventListener('mousemove', onMouseInput, {
      capture: true,
      passive: true,
      signal: state.controller.signal
    });

    element.addEventListener('mouseup', onMouseInput, {
      capture: true,
      passive: false,
      signal: state.controller.signal
    });
  };

  document.addEventListener(
    'pointerlockchange',
    onPointerLockChange,
    { passive: true });

  return {
    capture() {
      return captureSnapshot(performance.now(), state);
    },

    async requestLock() {
      if (document.pointerLockElement === element) {
        return;
      }

      try {
        await (element.requestPointerLock({ unadjustedMovement: true }) ?? element.requestPointerLock());
      } catch (e) {
        if (e instanceof Error && e.name === 'NotSupportedError') {
          return element.requestPointerLock();
        }

        throw e;
      }
    },

    dispose() {
      state.controller?.abort();
      document.removeEventListener('pointerlockchange', onPointerLockChange);
    }
  };
}

const captureSnapshot = (now: DOMHighResTimeStamp, state: UserInputState): UserInputSnapshot => {
  const snapshot: UserInputSnapshot = {
    delta: { ...state.delta },
    latency: state.now ? now - state.now : 0,
    pressed: [...state.pressed],
    released: [...state.released],
  };

  clearState(state, now);
  return snapshot;
}

const clearState = (state: UserInputState, now?: DOMHighResTimeStamp) => {
  state.delta = {
    x: 0,
    y: 0
  };

  state.now = now ?? 0;
  state.pressed.clear();
  state.released.clear();
}

const createAbortController = (onAbort: () => void): AbortController => {
  const controller = new AbortController();
  controller.signal.addEventListener(
    'abort',
    onAbort,
    { passive: true });

  return controller;
}

type UserInputSnapshot = {
  delta: { x: number; y: number; };
  latency: number;
  pressed: Array<string>;
  released: Array<string>;
}

type UserInputState = {
  controller?: AbortController;
  delta: { x: number; y: number; };
  now: DOMHighResTimeStamp;
  pressed: Set<string>;
  released: Set<string>;
}