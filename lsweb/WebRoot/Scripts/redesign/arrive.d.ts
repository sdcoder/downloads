interface JQuery {
    arrive(selector: string, callback: (newElem: JQuery) => void): JQuery;
    arrive(selector: string, options: ArriveOptions, callback: (newElem: JQuery) => void): JQuery;

    leave(selector: string, callback: (newElem: JQuery) => void): JQuery;

    unbindArrive(): JQuery;
    unbindArrive(selector: string): JQuery;
    unbindArrive(callback: () => void): JQuery;
    unbindArrive(selector: string, callback: () => void): JQuery;

    unbindLeave(): JQuery;
    unbindLeave(selector: string): JQuery;
    unbindLeave(callback: () => void): JQuery;
    unbindLeave(selector: string, callback: () => void): JQuery;
}

interface Document {
    arrive(selector: string, callback: () => void): void;
    arrive(selector: string, options: ArriveOptions, callback: () => void): void;

    leave(selector: string, callback: () => void): void;

    unbindArrive(): void;
    unbindArrive(selector: string): void;
    unbindArrive(callback: () => void): void;
    unbindArrive(selector: string, callback: () => void): void;

    unbindLeave(): void;
    unbindLeave(selector: string): void;
    unbindLeave(callback: () => void): void;
    unbindLeave(selector: string, callback: () => void): void;
}

interface Window {
    arrive(selector: string, callback: () => void): void;
    arrive(selector: string, options: ArriveOptions, callback: () => void): void;

    leave(selector: string, callback: () => void): void;

    unbindArrive(): void;
    unbindArrive(selector: string): void;
    unbindArrive(callback: () => void): void;
    unbindArrive(selector: string, callback: () => void): void;

    unbindLeave(): void;
    unbindLeave(selector: string): void;
    unbindLeave(callback: () => void): void;
    unbindLeave(selector: string, callback: () => void): void;
}

interface HTMLElement {
    arrive(selector: string, callback: () => void): void;
    arrive(selector: string, options: ArriveOptions, callback: () => void): void;

    leave(selector: string, callback: () => void): void;

    unbindArrive(): void;
    unbindArrive(selector: string): void;
    unbindArrive(callback: () => void): void;
    unbindArrive(selector: string, callback: () => void): void;

    unbindLeave(): void;
    unbindLeave(selector: string): void;
    unbindLeave(callback: () => void): void;
    unbindLeave(selector: string, callback: () => void): void;
}

interface ArriveOptions {
    fireOnAttributesModification: boolean;
    onceOnly: boolean;
    exiting: boolean;
}

interface Arrive {
    unbindAllArrive(): void;
    unbindAllLeave(): void;
}