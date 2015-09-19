class HelloMessage {
    private propers : string;

    public render() {
        return <div>Hello {this.props.name}</div>;
    }
}