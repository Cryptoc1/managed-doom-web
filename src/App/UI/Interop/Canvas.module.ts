export function createContext(canvas: HTMLCanvasElement) {
  const context = canvas.getContext("2d", {
    alpha: false,
    desynchronized: true,
    willReadFrequently: false,
  })!;

  const image = context.createImageData(canvas.width, canvas.height);
  return {
    getBoundingClientRect() {
      return canvas.getBoundingClientRect();
    },

    putImageData(data: Uint8Array) {
      console.log('putImageData', data.length);

      image.data.set(data);
      // context.putImageData(image, 0, 0);
    }
  };
}