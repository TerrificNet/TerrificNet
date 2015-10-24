interface VTypeIdentifier {
    version: string;
    type: string;
}

interface VNode extends VTypeIdentifier {
    tagName: string;
    properties: any;
    children: Array<VNode>;
}

interface VirtualText extends VTypeIdentifier {
    text: string;
}

declare module "virtual-dom/create-element" {
    function createElement(vnode: VNode, opts?: any): Element;

    export = createElement;
}

declare module "virtual-dom/vnode/vnode" {
    export = VNode;
}

declare module "virtual-dom/vnode/vtext" {
    export = VirtualText;
}
