export const storage = {
    get: (k: string) => localStorage.getItem(k),
    set: (k: string, v: string) => localStorage.setItem(k, v),
    del: (k: string) => localStorage.removeItem(k)
    }