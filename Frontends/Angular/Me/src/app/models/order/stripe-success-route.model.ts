export interface IStripeSuccessComponent {
    isStripeSuccessComponent(): this is IStripeSuccessComponent;
}

/**
 * Route components are uninstantiated class Type at runtime. To determine if the class is of this type
 * you must either instatiate it and check the obj for the method OR parse the class definition
 * for the existence of the method
 * @param component from the Route definition
 * @returns true if the component is determined to be of type IStripeSuccessComponent, else false
 */
export function isStripeSuccessComponent(component: IStripeSuccessComponent): component is IStripeSuccessComponent {
    return (undefined !== component && -1 < component.toString().indexOf("isStripeSuccessComponent()"));
}