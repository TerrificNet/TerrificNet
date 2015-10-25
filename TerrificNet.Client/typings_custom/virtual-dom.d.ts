interface VTypeIdentifier {
    version: string;
    type: string;
}

interface VTree extends VTypeIdentifier {
}

interface VNode extends VTree {
    tagName: string;
    properties: any;
    children: Array<VNode>;
}

interface VirtualText extends VTree {
    text: string;
}

interface PatchObject extends VTypeIdentifier {
    
}

declare module "virtual-dom/create-element" {
    function createElement(vnode: VTree, opts?: any): Node;

    export = createElement;
}

declare module "virtual-dom/vnode/vnode" {
    export = VNode;
}

declare module "virtual-dom/vnode/vtext" {
    export = VirtualText;
}

declare module "virtual-dom/diff" {
    function diff(previous: VTree, current: VTree): PatchObject;

    export = diff;
}

declare module "virtual-dom/patch" {
    function patch(rootNode: Node, patches: PatchObject): Node;

    export = patch;
}
